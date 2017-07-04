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
Imports scg = System.Collections.Generic
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Extensions.Data
   <sxs.XmlRoot("row", Namespace:=Constants.Namespaces.com),
    Attributes.CmisTypeInfo("com:row", Nothing, "row"),
    Attributes.JavaScriptConverter(GetType(JSON.Extensions.Data.RowConverter))>
   Public Class Row
      Inherits Serialization.XmlSerializable

#Region "Constructors"
      Public Sub New()
      End Sub

      Public Sub New(ParamArray properties As Core.Properties.cmisProperty())
         If properties IsNot Nothing Then
            _properties = New Core.Collections.cmisPropertiesType(properties)
         End If
      End Sub

      Friend Sub New(owner As RowCollection, ParamArray properties As Core.Properties.cmisProperty())
         Me.New(properties)
         SetOwner(owner)
      End Sub
#End Region

#Region "Helper classes"
      Private Class RowIndexHelper
         Inherits CmisObjectModel.Core.Properties.cmisPropertyInteger

         Public Sub New(row As Row)
            MyBase._values = New xs_Integer() {Integer.MaxValue}
            _row = row
            Me.RefreshRowIndex()
         End Sub

         Private WithEvents _innerProperty As CmisObjectModel.Core.Properties.cmisPropertyInteger

         ''' <summary>
         ''' Updates row-dependencies
         ''' </summary>
         Private Sub RefreshEventHandler()
            _row_Properties = _row._properties
            _rows = _row._owner
         End Sub

         Private Sub RefreshRowIndex()
            Dim rowIndexPropertyDefinitionId As String

            Me.RefreshEventHandler()
            rowIndexPropertyDefinitionId = If(_rows Is Nothing, Nothing, _rows.RowIndexPropertyDefinitionId)
            If _row_Properties Is Nothing OrElse
               String.IsNullOrEmpty(rowIndexPropertyDefinitionId) Then
               _innerProperty = Nothing
            Else
               Dim properties As scg.Dictionary(Of String, CmisObjectModel.Core.Properties.cmisProperty) = _row_Properties.GetProperties()
               _innerProperty = If(properties.ContainsKey(rowIndexPropertyDefinitionId),
                                   TryCast(properties(rowIndexPropertyDefinitionId), CmisObjectModel.Core.Properties.cmisPropertyInteger),
                                   Nothing)
            End If
            If _innerProperty IsNot Nothing Then Me.Value = _innerProperty.Value
         End Sub

         ''' <summary>
         ''' Observes events that may involve a change of the rowIndex value
         ''' </summary>
         Private Sub RefreshRowIndex(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles _row.PropertyChanged,
                                                                                                                    _row_Properties.PropertyChanged,
                                                                                                                    _rows.PropertyChanged
            'the rowIndex perhaps changed, if
            ' - the ownership of the row changed
            ' - the properties of the row or the properties-array of _row_Properties changed
            ' - the rowIndexPropertyDefinitionId
            If String.Compare(e.PropertyName, "Owner", True) = 0 OrElse
               String.Compare(e.PropertyName, "Properties", True) = 0 OrElse
               String.Compare(e.PropertyName, "RowIndexPropertyDefinitionId", True) = 0 Then
               Me.RefreshRowIndex()
            End If
         End Sub

         Private WithEvents _row As Row
         Private WithEvents _row_Properties As CmisObjectModel.Core.Collections.cmisPropertiesType
         Private WithEvents _rows As RowCollection
         Private _uncommittedValue As xs_Integer? = Nothing

         Public Overrides Property Value As xs_Integer
            Get
               Return If(_uncommittedValue.HasValue, _uncommittedValue.Value, MyBase.Value)
            End Get
            Set(value As xs_Integer)
               If MyBase.Value <> value Then
                  If _rows Is Nothing OrElse _rows.IsBusy Then
                     MyBase.Value = value
                  Else
                     'respect bounds
                     value = _rows.GetValidRowIndex(CInt(value))
                     _uncommittedValue = MyBase.Value
                     MyBase.Value = value
                     'refresh position in rowCollection
                     _rows.MoveRow(_row, CInt(value))
                     _uncommittedValue = Nothing
                  End If
                  'update innerProperty, if available and necessary
                  If _innerProperty IsNot Nothing AndAlso _innerProperty.Value <> value Then _innerProperty.Value = value
               End If
            End Set
         End Property

         Private Sub ValueChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles _innerProperty.PropertyChanged
            If _innerProperty IsNot Nothing Then
               Me.Value = _innerProperty.Value
            End If
         End Sub
      End Class
#End Region

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _rowState = ReadEnum(reader, attributeOverrides, "rowState", Constants.Namespaces.com, _rowState)
         _properties = Read(Of Core.Collections.cmisPropertiesType)(reader, attributeOverrides, "properties", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Collections.cmisPropertiesType))
         Select Case _rowState
            Case DataRowState.Deleted, DataRowState.Modified
               _originalProperties = Read(Of Core.Collections.cmisPropertiesType)(reader, attributeOverrides, "originalProperties", Constants.Namespaces.com, AddressOf GenericXmlSerializableFactory(Of Core.Collections.cmisPropertiesType))
            Case DataRowState.Unchanged
               If _properties IsNot Nothing Then
                  _originalProperties = CType(_properties.Copy(), CmisObjectModel.Core.Collections.cmisPropertiesType)
               End If
               Me.AddHandler()
         End Select
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "rowState", Constants.Namespaces.com, _rowState.GetName())
         If _propertiesSupport.Contains(_rowState) Then WriteElement(writer, attributeOverrides, "properties", Constants.Namespaces.cmis, _properties)
         'serialization of originalProperties only for states which support differences between
         'properties and originalProperties
         If _rowState = DataRowState.Deleted OrElse _rowState = DataRowState.Modified Then
            WriteElement(writer, attributeOverrides, "originalProperties", Constants.Namespaces.com, _originalProperties)
         End If
      End Sub
#End Region

#Region "Observe Properties"
      Private _observedSerializables As New List(Of Serialization.XmlSerializable)
      ''' <summary>
      ''' Setup the necessary event handlers to observe the rowState
      ''' </summary>
      ''' <remarks></remarks>
      Private Overloads Sub [AddHandler]()
         Me.RemoveHandler()
         If _properties IsNot Nothing Then
            Me.AddHandler(_properties)
            If _properties.Properties IsNot Nothing Then
               For Each prop As Core.Properties.cmisProperty In _properties.Properties
                  If prop IsNot Nothing Then Me.AddHandler(prop)
               Next
            End If
         End If
      End Sub
      Private Overloads Sub [AddHandler](observedSerializable As Serialization.XmlSerializable)
         If TypeOf observedSerializable Is Core.Collections.cmisPropertiesType Then
            observedSerializable.AddHandler(AddressOf _observedSerializable_PropertyChanged, "Properties")
         Else
            observedSerializable.AddHandler(AddressOf _observedSerializable_PropertyChanged, "Values")
         End If
         _observedSerializables.Add(observedSerializable)
      End Sub

      ''' <summary>
      ''' Event-Dispatcher
      ''' </summary>
      ''' <param name="sender"></param>
      ''' <param name="e"></param>
      ''' <remarks></remarks>
      Private Sub _observedSerializable_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
         SetRowState(DataRowState.Modified)
      End Sub

      ''' <summary>
      ''' Removes necessary event handlers to observe the rowState
      ''' </summary>
      ''' <remarks></remarks>
      Private Overloads Sub [RemoveHandler]()
         For Each observedSerializable As Serialization.XmlSerializable In _observedSerializables
            If TypeOf observedSerializable Is Core.Collections.cmisPropertiesType Then
               observedSerializable.RemoveHandler(AddressOf _observedSerializable_PropertyChanged, "Properties")
            Else
               observedSerializable.RemoveHandler(AddressOf _observedSerializable_PropertyChanged, "Values")
            End If
         Next
         _observedSerializables.Clear()
      End Sub
#End Region

#Region "Methods to modify the row state"
      Public Sub AcceptChanges()
         SetRowState(DataRowState.Unchanged)
      End Sub

      Public Sub Delete()
         SetRowState(DataRowState.Deleted)
      End Sub

      Public Sub RejectChanges()
         _properties = _originalProperties
         SetRowState(DataRowState.Unchanged)
      End Sub

      Public Event RowStateChanged As EventHandler

      Public Sub SetAdded()
         SetRowState(DataRowState.Added)
      End Sub

      Public Sub SetModified()
         SetRowState(DataRowState.Modified)
      End Sub

      Private Sub SetRowState(value As DataRowState)
         If value <> _rowState Then
            Dim oldValue As DataRowState = _rowState
            Select Case value
               Case DataRowState.Added
                  If oldValue = DataRowState.Deleted Then _properties = _originalProperties
                  _originalProperties = Nothing
                  Me.RemoveHandler()
               Case DataRowState.Deleted
                  If oldValue = DataRowState.Added Then
                     value = DataRowState.Detached
                  Else
                     _properties = Nothing
                  End If
                  Me.RemoveHandler()
               Case DataRowState.Modified
                  Me.RemoveHandler()
               Case DataRowState.Unchanged
                  _originalProperties = If(_properties Is Nothing, Nothing, CType(_properties.Copy(), Core.Collections.cmisPropertiesType))
                  Me.AddHandler()
            End Select
            _rowState = value
            OnPropertyChanged("RowState", value, oldValue)
            RaiseEvent RowStateChanged(Me, EventArgs.Empty)
         End If
      End Sub
#End Region

      Friend Sub MapProperties(mapper As CmisObjectModel.Data.Mapper, direction As enumMapDirection,
                               rollbackSettings As Dictionary(Of Core.Properties.cmisProperty, Object()))
         If _originalProperties IsNot Nothing Then mapper.MapProperties(_originalProperties, direction, rollbackSettings)
         If _properties IsNot Nothing Then mapper.MapProperties(_properties, direction, rollbackSettings)
      End Sub

      Private Shared _originalPropertiesSupport As New HashSet(Of DataRowState) From {
         DataRowState.Deleted, DataRowState.Modified, DataRowState.Unchanged}
      Private _originalProperties As Core.Collections.cmisPropertiesType
      ''' <summary>
      ''' Returns a copy of the originalProperties if available, otherwise null
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetOriginalProperties() As Core.Collections.cmisPropertiesType
         Return If(_originalProperties IsNot Nothing AndAlso _originalPropertiesSupport.Contains(_rowState),
                   CType(_originalProperties.Copy(), Core.Collections.cmisPropertiesType), Nothing)
      End Function

      Private _owner As RowCollection
      ''' <summary>
      ''' Returns the row-collection the current instance belongs to
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.Web.Script.Serialization.ScriptIgnore()>
      Public ReadOnly Property Owner As RowCollection
         Get
            Return _owner
         End Get
      End Property
      Friend Sub SetOwner(owner As RowCollection)
         If _owner IsNot owner Then
            Dim oldValue As RowCollection = _owner
            _owner = owner
            OnPropertyChanged("Owner", owner, oldValue)
         End If
      End Sub

      Private Shared _propertiesSupport As New HashSet(Of DataRowState) From {
         DataRowState.Added, DataRowState.Detached, DataRowState.Modified, DataRowState.Unchanged}
      Protected _properties As Core.Collections.cmisPropertiesType
      ''' <summary>
      ''' The current properties of this instance
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Property Properties As Core.Collections.cmisPropertiesType
         Get
            Return If(_propertiesSupport.Contains(_rowState), _properties, Nothing)
         End Get
         Set(value As Core.Collections.cmisPropertiesType)
            If value IsNot _properties AndAlso _propertiesSupport.Contains(_rowState) Then
               Dim oldValue As Core.Collections.cmisPropertiesType = _properties
               _properties = value
               OnPropertyChanged("Properties", value, oldValue)
               If _rowState = DataRowState.Unchanged Then
                  SetRowState(DataRowState.Modified)
               ElseIf _observedSerializables.Count > 0 Then
                  Me.RemoveHandler()
               End If
            End If
         End Set
      End Property 'Properties

      Private _rowIndex As New RowIndexHelper(Me)
      Public Property RowIndex As Integer
         Get
            Return CInt(_rowIndex.Value)
         End Get
         Set(value As Integer)
            _rowIndex.Value = value
         End Set
      End Property

      Private _rowState As DataRowState = DataRowState.Detached
      ''' <summary>
      ''' The RowState of this instance
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property RowState As DataRowState
         Get
            Return _rowState
         End Get
      End Property 'RowState


      ''' <summary>
      ''' Initializes instance with given parameters
      ''' </summary>
      Protected Shared Sub SilentInitialization(instance As Row, properties As Core.Collections.cmisPropertiesType,
                                                originalProperties As Core.Collections.cmisPropertiesType,
                                                rowState As DataRowState)
         If instance IsNot Nothing Then
            instance.RemoveHandler()
            instance._rowState = rowState
            instance._properties = properties
            instance._originalProperties = originalProperties
            If rowState = DataRowState.Unchanged Then instance.AddHandler()
         End If
      End Sub

   End Class
End Namespace