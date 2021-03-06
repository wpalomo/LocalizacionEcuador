﻿Imports System.Xml

Public Class generarFRXML
    Public Sub generarXML(DocEntry As String, objectType As String, oCompany As SAPbobsCOM.Company, SBO As SAPbouiCOM.Application)
        Try
            Dim doc As New XmlDocument
            Dim oRecord As SAPbobsCOM.Recordset
            oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRecord.DoQuery("SELECT A.DocEntry  FROM OPCH A WHERE A.DocEntry=" & DocEntry & " and A.U_TI_COMPRO='41'")
            If oRecord.RecordCount > 0 Then
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec ENCABEZADO_FACTURA '" & DocEntry & "','FR'")
                Dim writer As New XmlTextWriter("Comprobante (FR) No." & DocEntry.ToString & ".xml", System.Text.Encoding.UTF8)
                writer.WriteStartDocument(True)
                writer.Formatting = Formatting.Indented
                writer.Indentation = 2
                writer.WriteStartElement("factura")
                writer.WriteAttributeString("id", "comprobante")
                writer.WriteAttributeString("version", "2.0.0")
                writer.WriteStartElement("infoTributaria")
                createNode("razonSocial", oRecord.Fields.Item(2).Value.ToString, writer)
                'createNode("ambiente", oRecord.Fields.Item(0).Value.ToString, writer)
                'createNode("tipoEmision", oRecord.Fields.Item(1).Value.ToString, writer)
                createNode("ruc", oRecord.Fields.Item(3).Value.ToString.PadLeft(13, "0"), writer)
                'createNode("claveAcesso", claveAcceso(oRecord).PadLeft(49, "0"), writer)
                'createNode("claveAcesso", "", writer)
                createNode("codDoc", oRecord.Fields.Item("codDoc").Value.ToString.PadLeft(2, "0"), writer)
                createNode("estab", oRecord.Fields.Item("estable").Value.ToString.PadLeft(3, "0"), writer)
                createNode("ptoEmi", oRecord.Fields.Item("ptoemi").Value.ToString.PadLeft(3, "0"), writer)
                createNode("secuencial", oRecord.Fields.Item("secuencial").Value.ToString.PadLeft(9, "0"), writer)
                createNode("dirMatriz", oRecord.Fields.Item("dirMatriz").Value.ToString, writer)
                Dim direccion = oRecord.Fields.Item("dirMatriz").Value.ToString
                Dim contribuyenteEspecial = oRecord.Fields.Item("contriespecial").Value.ToString
                Dim obliConta = oRecord.Fields.Item("contaobligado").Value.ToString
                ''Cierre info Tributaria
                writer.WriteEndElement()

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                writer.WriteStartElement("infoFactura")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec SP_INFO_FACTURA '" & DocEntry & "','FR'")
                createNode("fechaEmision", Date.Parse(oRecord.Fields.Item("DATE").Value.ToString).ToString("dd/MM/yyyy"), writer)
                createNode("dirEstablecimiento", direccion, writer)
                If contribuyenteEspecial <> "" Then
                    createNode("contribuyenteEspecial", contribuyenteEspecial, writer)
                End If
                createNode("obligadoContabilidad", oRecord.Fields.Item(3).Value, writer)
                createNode("tipoIdentificacionComprador", oRecord.Fields.Item("U_IDENTIFICACION").Value.ToString, writer)
                createNode("razonSocialComprador", oRecord.Fields.Item("CardName").Value.ToString, writer)
                createNode("identificacionComprador", oRecord.Fields.Item("U_DOCUMENTO").Value.ToString, writer)
                createNode("totalSinImpuestos", oRecord.Fields.Item("sin_impuesto").Value.ToString, writer)
                createNode("totalDescuento", oRecord.Fields.Item("totDescuento").Value.ToString, writer)
                createNode("codDocReembolso", "41", writer)
                Dim importeTotal = oRecord.Fields.Item("DocTotal").Value.ToString
                Dim moneda = oRecord.Fields.Item("MONEDA").Value.ToString
                createNode("totalComprobantesReembolso", importeTotal, writer)
                createNode("totalBaseImponibleReembolso", oRecord.Fields.Item("sin_impuesto").Value.ToString, writer)
                createNode("totalImpuestoReembolso", oRecord.Fields.Item("VatSum").Value.ToString, writer)
                writer.WriteStartElement("totalConImpuestos")
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec SP_Total_Con_Impuesto '" & DocEntry & "','FR'")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        writer.WriteStartElement("totalImpuesto")
                        createNode("codigo", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("codigoPorcentaje", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("baseImponible", oRecord.Fields.Item(2).Value.ToString, writer)
                        createNode("tarifa", oRecord.Fields.Item(3).Value, writer)
                        createNode("valor", oRecord.Fields.Item(4).Value.ToString, writer)
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                ''Cierre TotalConImpuestos
                writer.WriteEndElement()
                createNode("propina", "0.00", writer)
                createNode("importeTotal", importeTotal, writer)
                createNode("moneda", moneda, writer)
                writer.WriteStartElement("pagos")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec SP_Forma_Pago '" & DocEntry & "','FR'")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        writer.WriteStartElement("pago")
                        createNode("formaPago", oRecord.Fields.Item(0).Value, writer)
                        createNode("total", oRecord.Fields.Item(1).Value, writer)
                        createNode("plazo", oRecord.Fields.Item(2).Value, writer)
                        createNode("unidadTiempo", oRecord.Fields.Item(3).Value, writer)
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If
                ''Cierre Pagos
                writer.WriteEndElement()



                ''Cierre INFO FACTURA
                writer.WriteEndElement()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                writer.WriteStartElement("detalles")
                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec SP_DetalleFac '" & DocEntry & "','FR'")
                If oRecord.RecordCount > 0 Then
                    While oRecord.EoF = False
                        Dim oRecord2 As SAPbobsCOM.Recordset
                        oRecord2 = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        writer.WriteStartElement("detalle")
                        createNode("codigoPrincipal", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("descripcion", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("cantidad", oRecord.Fields.Item(2).Value.ToString, writer)
                        createNode("precioUnitario", oRecord.Fields.Item(3).Value.ToString, writer)
                        createNode("descuento", oRecord.Fields.Item(4).Value.ToString, writer)
                        createNode("precioTotalSinImpuesto", oRecord.Fields.Item(6).Value.ToString, writer)
                        writer.WriteStartElement("impuestos")
                        oRecord2.DoQuery("exec SP_Impuesto_Detalle '" & DocEntry & "','" & oRecord.Fields.Item(0).Value.ToString & "','FR'")
                        If oRecord2.RecordCount > 0 Then
                            While oRecord2.EoF = False
                                writer.WriteStartElement("impuesto")
                                createNode("codigo", oRecord2.Fields.Item(0).Value.ToString, writer)
                                createNode("codigoPorcentaje", oRecord2.Fields.Item(1).Value.ToString, writer)
                                createNode("tarifa", oRecord2.Fields.Item(3).Value.ToString, writer)
                                createNode("baseImponible", oRecord2.Fields.Item(2).Value.ToString, writer)
                                createNode("valor", oRecord2.Fields.Item(4).Value.ToString, writer)
                                writer.WriteEndElement()
                                oRecord2.MoveNext()
                            End While
                        End If

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord2)
                        oRecord2 = Nothing
                        GC.Collect()
                        writer.WriteEndElement()

                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While
                End If

                ''Cierre detalles
                writer.WriteEndElement()
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()


                writer.WriteStartElement("reembolsos")

                oRecord = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                oRecord.DoQuery("exec SP_DetalleFacReembolso '" & DocEntry & "','13'")
                If oRecord.RecordCount > 0 Then
                    While (oRecord.EoF = False)
                        writer.WriteStartElement("reembolsoDetalle")
                        Dim oRecord2 As SAPbobsCOM.Recordset
                        oRecord2 = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        createNode("tipoIdentificacionProveedorReembolso", oRecord.Fields.Item(0).Value.ToString, writer)
                        createNode("identificacionProveedorReembolso", oRecord.Fields.Item(1).Value.ToString, writer)
                        createNode("codPaisPagoProveedorReembolso", oRecord.Fields.Item(2).Value.ToString, writer)
                        createNode("tipoProveedorReembolso", oRecord.Fields.Item(3).Value.ToString, writer)
                        createNode("codDocReembolso", oRecord.Fields.Item(4).Value.ToString, writer)
                        createNode("estabDocReembolso", oRecord.Fields.Item(5).Value.ToString, writer)
                        createNode("ptoEmiDocReembolso", oRecord.Fields.Item(6).Value.ToString, writer)
                        createNode("secuencialDocReembolso", oRecord.Fields.Item(7).Value.ToString.PadLeft(9, "0"), writer)
                        createNode("fechaEmisionDocReembolso", oRecord.Fields.Item(8).Value.ToString, writer)
                        createNode("numeroautorizacionDocReemb", oRecord.Fields.Item(9).Value.ToString, writer)
                        writer.WriteStartElement("detalleImpuestos")
                        oRecord2.DoQuery("exec SP_Impuesto_Detalle '" & DocEntry & "','" & oRecord.Fields.Item(0).Value.ToString & "','FR'")
                        If oRecord2.RecordCount > 0 Then
                            While oRecord2.EoF = False
                                writer.WriteStartElement("detalleImpuesto")
                                createNode("codigo", oRecord2.Fields.Item(0).Value.ToString, writer)
                                createNode("codigoPorcentaje", oRecord2.Fields.Item(1).Value.ToString, writer)
                                createNode("tarifa", oRecord2.Fields.Item(3).Value.ToString, writer)
                                createNode("baseImponibleReembolso", oRecord2.Fields.Item(2).Value.ToString, writer)
                                createNode("impuestoReembolso", oRecord2.Fields.Item(4).Value.ToString, writer)
                                writer.WriteEndElement()
                                oRecord2.MoveNext()
                            End While

                        End If

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord2)
                        oRecord2 = Nothing
                        GC.Collect()
                        writer.WriteEndElement()
                        oRecord.MoveNext()
                    End While

                    ''Cierre Reembolso
                    writer.WriteEndElement()
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()

                ''Cierre Reembolso
                writer.WriteEndElement()

                ''Cierre Factura
                writer.WriteEndElement()
                writer.WriteEndDocument()
                writer.Close()
            Else
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecord)
                oRecord = Nothing
                GC.Collect()
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Private Sub createNode(ByVal pID As String, ByVal pName As String, ByVal writer As XmlTextWriter)
        writer.WriteStartElement(pID)
        writer.WriteString(pName)
        writer.WriteEndElement()
    End Sub
End Class
