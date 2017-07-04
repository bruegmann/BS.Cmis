'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Extensions.Data
   ''' <summary>
   ''' Support for structured database contents
   ''' </summary>
   ''' <remarks></remarks>
   <sxs.XmlRoot("rowCollection", Namespace:=Constants.Namespaces.com),
    Attributes.CmisTypeInfo("com:rowCollection", Nothing, "rowCollection"),
    Attributes.JavaScriptConverter(GetType(JSON.Extensions.Data.RowCollectionConverter))>
   Public Class RowCollection
      Inherits Extension

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of RowCollection, String)) From {
         {"rowindexpropertydefinitionid", AddressOf SetRowIndexPropertyDefinitionId},
         {"rowtypeid", AddressOf SetRowTypeId},
         {"tablename", AddressOf SetTableName}} '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in attributes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
         'at least one property is serialized in an attribute-value
         If _setter.Count > 0 Then
            For attributeIndex As Integer = 0 To reader.AttributeCount - 1
               reader.MoveToAttribute(attributeIndex)
               'attribute name
               Dim key As String = reader.Name.ToLowerInvariant
               If _setter.ContainsKey(key) Then _setter(key).Invoke(Me, reader.GetAttribute(attributeIndex))
            Next
         End If
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         Dim rows As Row() = ReadArray(Of Row)(reader, attributeOverrides, "row", Constants.Namespaces.com, AddressOf GenericXmlSerializableFactory(Of Row))

         Try
            _busy += 1
            If rows IsNot Nothing Then
               For Each row As Row In rows
                  If row IsNot Nothing Then
                     If row.RowState = DataRowState.Detached Then row.SetAdded()
                     row.SetOwner(Me)
                     _rows.Add(row)
                     AddHandler row.RowStateChanged, AddressOf _row_RowStateChanged
                  End If
               Next
            End If
         Finally
            _busy -= 1
         End Try
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteAttribute(writer, attributeOverrides, "tableName", Nothing, _tableName)
         WriteAttribute(writer, attributeOverrides, "rowTypeId", Nothing, _rowTypeId)
         WriteAttribute(writer, attributeOverrides, "rowIndexPropertyDefinitionId", Nothing, _RowIndexPropertyDefinitionId)
         If _rows.Count > 0 Then WriteArray(writer, attributeOverrides, "row", Constants.Namespaces.com, _rows.ToArray())
      End Sub
#End Region

#Region "LoadData"
      ''' <summary>
      ''' Informs the collection that data loading starts
      ''' </summary>
      Public Sub BeginLoadData()
         _busy += 1
      End Sub

      Private _busy As Integer = 0

      ''' <summary>
      ''' Informs the collection that data loading is completed
      ''' </summary>
      Public Sub EndLoadData()
         If _busy = 1 Then
            SortRows()
            _busy = 0
            If _rows.Count > 0 Then OnPropertyChanged("Rows")
         ElseIf _busy > 1 Then
            _busy -= 1
         End If
      End Sub

      Public ReadOnly Property IsBusy As Boolean
         Get
            Return _busy > 0
         End Get
      End Property
#End Region

      ''' <summary>
      ''' Sets all rows to RowState unChanged
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub AcceptChanges()
         For Each row As Row In _rows
            row.AcceptChanges()
         Next
      End Sub

      ''' <summary>
      ''' Adds a row instance to the collection if the has no owner by now
      ''' </summary>
      ''' <param name="row"></param>
      ''' <remarks></remarks>
      Public Sub AddRow(row As Row)
         InsertRow(_rows.Count, row)
      End Sub

      ''' <summary>
      ''' Returns rowIndex-value in valid range
      ''' </summary>
      Friend Function GetValidRowIndex(proposedRowIndex As Integer) As Integer
         Return Math.Min(Math.Max(0, proposedRowIndex), _rows.Count - 1)
      End Function

      Public Sub InsertRow(rowIndex As Integer, row As Row)
         If row IsNot Nothing AndAlso row.Owner Is Nothing Then
            Try
               _busy += 1
               rowIndex = Math.Min(Math.Max(0, rowIndex), _rows.Count)
               row.SetOwner(Me)
               row.SetAdded()
               If rowIndex >= _rows.Count Then
                  _rows.Add(row)
               Else
                  _rows.Insert(rowIndex, row)
               End If
               AddHandler row.RowStateChanged, AddressOf _row_RowStateChanged
               If _busy = 1 Then
                  For index As Integer = rowIndex To _rows.Count - 1
                     _rows(index).RowIndex = index
                  Next
                  OnPropertyChanged("Rows")
               End If
            Finally
               _busy -= 1
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Build a new row with the given properties
      ''' </summary>
      ''' <param name="fAcceptChanges"></param>
      ''' <param name="properties"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function LoadRow(fAcceptChanges As Boolean, ParamArray properties As CmisObjectModel.Core.Properties.cmisProperty()) As Row
         Try
            _busy += 1

            Dim retVal As New Row(Me, properties)

            If fAcceptChanges Then
               retVal.AcceptChanges()
            Else
               retVal.SetAdded()
            End If
            _rows.Add(retVal)
            AddHandler retVal.RowStateChanged, AddressOf _row_RowStateChanged
            If _busy = 1 Then
               retVal.RowIndex = _rows.Count - 1
               OnPropertyChanged("Rows")
            End If

            Return retVal
         Finally
            _busy -= 1
         End Try
      End Function

      ''' <summary>
      ''' Moves row to the specified rowIndex
      ''' </summary>
      Public Sub MoveRow(row As Row, rowIndex As Integer)
         Dim currentRowIndex As Integer

         If _busy = 0 AndAlso row IsNot Nothing AndAlso row.Owner Is Me Then
            currentRowIndex = GetValidRowIndex(row.RowIndex)
            rowIndex = GetValidRowIndex(rowIndex)
            If currentRowIndex < 0 OrElse _rows(currentRowIndex) IsNot row Then
               currentRowIndex = _rows.IndexOf(row)
            End If
            If currentRowIndex >= 0 AndAlso currentRowIndex <> rowIndex Then
               Try
                  _busy += 1
                  If currentRowIndex < rowIndex Then
                     For index As Integer = currentRowIndex To rowIndex - 1
                        _rows(index) = _rows(index + 1)
                        _rows(index).RowIndex = index
                     Next
                  Else
                     For index As Integer = currentRowIndex To rowIndex + 1 Step -1
                        _rows(index) = _rows(index - 1)
                        _rows(index).RowIndex = index
                     Next
                  End If
                  _rows(rowIndex) = row
                  row.RowIndex = rowIndex
               Finally
                  _busy -= 1
               End Try
            End If
         End If
      End Sub

      ''' <summary>
      ''' Removes a row if the rowState turned to Detached
      ''' </summary>
      ''' <param name="sender"></param>
      ''' <param name="e"></param>
      ''' <remarks></remarks>
      Private Sub _row_RowStateChanged(sender As Object, e As EventArgs)
         Dim row As Row = CType(sender, Row)

         If row.RowState = DataRowState.Detached Then
            Dim rowIndex As Integer = GetValidRowIndex(row.RowIndex)

            RemoveHandler row.RowStateChanged, AddressOf _row_RowStateChanged
            If rowIndex < 0 OrElse _rows(rowIndex) IsNot row Then
               rowIndex = _rows.IndexOf(row)
            End If
            If rowIndex >= 0 Then
               _rows.RemoveAt(rowIndex)
               row.SetOwner(Nothing)
               Try
                  _busy += 1
                  For index As Integer = rowIndex To _rows.Count - 1
                     _rows(index).RowIndex = index
                  Next
               Finally
                  _busy -= 1
               End Try
               OnPropertyChanged("Rows")
            End If
         End If
      End Sub

      Private _rowIndexPropertyDefinitionId As String
      Public Property RowIndexPropertyDefinitionId As String
         Get
            Return _rowIndexPropertyDefinitionId
         End Get
         Set(value As String)
            If value <> _rowIndexPropertyDefinitionId Then
               Dim oldValue As String = _rowIndexPropertyDefinitionId
               _rowIndexPropertyDefinitionId = value
               OnPropertyChanged("RowIndexPropertyDefinitionId", value, oldValue)
            End If
         End Set
      End Property 'RowIndexPropertyDefinitionId
      Private Shared Sub SetRowIndexPropertyDefinitionId(instance As RowCollection, value As String)
         instance._rowIndexPropertyDefinitionId = value
      End Sub

      Private _rows As New List(Of Row)
      ''' <summary>
      ''' RowCollection
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Rows As Row()
         Get
            Return _rows.ToArray()
         End Get
      End Property 'Rows

      Private _rowTypeId As String
      ''' <summary>
      ''' TypeId to describe the properties of the row
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>Secondary Type recommend</remarks>
      Public Property RowTypeId As String
         Get
            Return _rowTypeId
         End Get
         Set(value As String)
            If _rowTypeId <> value Then
               Dim oldValue As String = _rowTypeId
               _rowTypeId = value
               OnPropertyChanged("RowTypeId", value, oldValue)
            End If
         End Set
      End Property 'RowTypeId
      Private Shared Sub SetRowTypeId(instance As RowCollection, value As String)
         instance.RowTypeId = value
      End Sub

      ''' <summary>
      ''' Initializes instance with given parameters
      ''' </summary>
      Protected Shared Sub SilentInitialization(instance As CmisObjectModel.Extensions.Data.RowCollection,
                                                rowIndexPropertyDefinitionId As String,
                                                rowTypeId As String, tableName As String,
                                                rows As CmisObjectModel.Extensions.Data.Row())
         If instance IsNot Nothing Then
            Try
               instance._busy += 1
               instance._rowIndexPropertyDefinitionId = rowIndexPropertyDefinitionId
               instance._rowTypeId = rowTypeId
               instance._tableName = tableName
               instance._rows.Clear()
               If rows IsNot Nothing Then
                  For Each row As Row In rows
                     If row IsNot Nothing Then
                        If row.RowState = DataRowState.Detached Then row.SetAdded()
                        row.SetOwner(instance)
                        instance._rows.Add(row)
                        AddHandler row.RowStateChanged, AddressOf instance._row_RowStateChanged
                     End If
                  Next
               End If
            Finally
               instance._busy -= 1
            End Try
         End If
      End Sub

      Private Sub SortRows()
         Try
            _busy += 1
            If Not String.IsNullOrEmpty(_rowIndexPropertyDefinitionId) Then
               _rows.Sort(Function(first, second) first.RowIndex - second.RowIndex)
            End If
            For index As Integer = 0 To _rows.Count - 1
               _rows(index).RowIndex = index
            Next
         Finally
            _busy -= 1
         End Try
      End Sub

      Private _tableName As String
      Public Property TableName As String
         Get
            Return _tableName
         End Get
         Set(value As String)
            If _tableName <> value Then
               Dim oldValue As String = _tableName
               _tableName = value
               OnPropertyChanged("TableName", value, oldValue)
            End If
         End Set
      End Property 'TableName
      Private Shared Sub SetTableName(instance As RowCollection, value As String)
         instance.TableName = value
      End Sub

   End Class
End Namespace