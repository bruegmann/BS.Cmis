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
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel

Namespace CmisObjectModel.Core.Properties
   <Attributes.JavaScriptObjectResolver(GetType(JSON.Serialization.CmisPropertyResolver))>
   Partial Public Class cmisProperty
      Implements IComparable, IComparable(Of cmisProperty)

#Region "Constructors"
      Shared Sub New()
         'search for all types supporting cmisProperties ...
         If Not ExploreAssembly(GetType(cmisProperty).Assembly) Then
            '... failed.
            'At least register well-known cmisProperty-classes
            cac.ExploreTypes(Of cmisProperty)(
               _factories, _genericTypeDefinition,
               GetType(Properties.cmisPropertyBoolean),
               GetType(Properties.cmisPropertyDateTime),
               GetType(Properties.cmisPropertyDecimal),
               GetType(Properties.cmisPropertyDouble),
               GetType(Properties.cmisPropertyHtml),
               GetType(Properties.cmisPropertyId),
               GetType(Properties.cmisPropertyInteger),
               GetType(Properties.cmisPropertyObject),
               GetType(Properties.cmisPropertyString),
               GetType(Properties.cmisPropertyUri))
            'update the FromType()-support
            ExploreFactories()
         End If

         AddHandler Common.DecimalRepresentationChanged, AddressOf OnDecimalRepresentationChanged
         If Common.DecimalRepresentation <> enumDecimalRepresentation.decimal Then
            OnDecimalRepresentationChanged(Common.DecimalRepresentation)
         End If
      End Sub
      Protected Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String)
         Me.PropertyDefinitionId = propertyDefinitionId
         Me.LocalName = localName
         Me.DisplayName = displayName
         Me.QueryName = queryName
      End Sub

      ''' <summary>
      ''' Creates a CmisProperty-instance from the current node of the reader-object using the
      ''' name of the current node to determine the suitable type
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As System.Xml.XmlReader) As cmisProperty
         reader.MoveToContent()

         Dim nodeName As String = reader.LocalName
         If nodeName <> "" Then
            nodeName = nodeName.ToLowerInvariant()
            If _factories.ContainsKey(nodeName) Then
               Dim retVal As cmisProperty = _factories(nodeName).CreateInstance()

               If retVal IsNot Nothing Then
                  retVal.ReadXml(reader)
                  Return retVal
               End If
            End If
         End If

         'current node doesn't describe a CmisProperty-instance
         Return Nothing
      End Function

      ''' <summary>
      ''' Searches in assembly for types supporting cmisProperties
      ''' </summary>
      ''' <param name="assembly"></param>
      ''' <remarks></remarks>
      Public Shared Function ExploreAssembly(assembly As System.Reflection.Assembly) As Boolean
         Try
            'explore the complete assembly if possible
            If assembly IsNot Nothing Then cac.ExploreTypes(Of cmisProperty)(_factories, _genericTypeDefinition, assembly.GetTypes())
            'update FromType-support
            ExploreFactories()
            Return True
         Catch
            Return False
         End Try
      End Function

      ''' <summary>
      ''' Updates the FromType() support
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared Sub ExploreFactories()
         Try
            Dim factories As New HashSet(Of CmisObjectModel.Common.Generic.Factory(Of cmisProperty))(_fromTypeFactories.Values)
            Dim addMethod As Action(Of CmisObjectModel.Common.Generic.Factory(Of cmisProperty)) =
               Sub(factory)
                  Dim [property] As cmisProperty = If(factories.Add(factory), factory.CreateInstance(), Nothing)
                  Dim propertyType As Type = If([property] Is Nothing, Nothing, [property].PropertyType)

                  If Not (propertyType Is Nothing OrElse _fromTypeFactories.ContainsKey(propertyType)) Then
                     _fromTypeFactories.Add(propertyType, factory)
                     _fromTypeFactories.Add(propertyType.MakeArrayType(), factory)
                  End If
               End Sub

            'preferred factories
            For Each priorityFactory As String In _priorityFactories
               If _factories.ContainsKey(priorityFactory) Then addMethod.Invoke(_factories(priorityFactory))
            Next
            'check the rest
            For Each factory As CmisObjectModel.Common.Generic.Factory(Of cmisProperty) In _factories.Values
               addMethod.Invoke(factory)
            Next
            'support for DateTime,Int32/Int64-class
            If Not _fromTypeFactories.ContainsKey(GetType(DateTime)) AndAlso
               _fromTypeFactories.ContainsKey(GetType(DateTimeOffset)) Then
               _fromTypeFactories.Add(GetType(DateTime), _fromTypeFactories(GetType(DateTimeOffset)))
               _fromTypeFactories.Add(GetType(DateTime()), _fromTypeFactories(GetType(DateTimeOffset())))
            End If
            If Not _fromTypeFactories.ContainsKey(GetType(Int32)) AndAlso
               _fromTypeFactories.ContainsKey(GetType(Int64)) Then
               _fromTypeFactories.Add(GetType(Int32), _fromTypeFactories(GetType(Int64)))
               _fromTypeFactories.Add(GetType(Int32()), _fromTypeFactories(GetType(Int64())))
            End If
            If Not _fromTypeFactories.ContainsKey(GetType(Int64)) AndAlso
               _fromTypeFactories.ContainsKey(GetType(Int32)) Then
               _fromTypeFactories.Add(GetType(Int64), _fromTypeFactories(GetType(Int32)))
               _fromTypeFactories.Add(GetType(Int64()), _fromTypeFactories(GetType(Int32())))
            End If
         Catch
         End Try
      End Sub

      Protected Shared _factories As New Dictionary(Of String, CmisObjectModel.Common.Generic.Factory(Of cmisProperty))
      Protected Shared _fromTypeFactories As New Dictionary(Of Type, CmisObjectModel.Common.Generic.Factory(Of cmisProperty))

      ''' <summary>
      ''' Returns a cmisProperty-instance for given type
      ''' </summary>
      Public Shared Function FromType(type As Type) As cmisProperty
         While type IsNot Nothing
            If _fromTypeFactories.ContainsKey(type) Then
               Return _fromTypeFactories(type).CreateInstance()
            Else
               type = type.BaseType
            End If
         End While

         'no cmisProperty can represent this type
         Return Nothing
      End Function

      ''' <summary>
      ''' GetType(Generic.Factory(Of CmisProperty, TDerivedFromCmisProperty))
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _genericTypeDefinition As Type = GetType(CmisObjectModel.Common.Generic.Factory(Of cmisProperty, cmisPropertyBoolean)).GetGenericTypeDefinition
      Private Shared _priorityFactories As String() = New String() {Properties.cmisPropertyString.CmisTypeName.ToLowerInvariant(),
                                                                    Properties.cmisPropertyString.DefaultElementName.ToLowerInvariant(),
                                                                    Properties.cmisPropertyString.TargetTypeName.ToLowerInvariant()}
#End Region

#Region "INotifyPropertiesChanged"
      Public Event ExtendedPropertyChanged(sender As Object, e As sc.PropertyChangedEventArgs)
      Protected Sub OnExtendedPropertyChanged(propertyName As String)
         RaiseEvent ExtendedPropertyChanged(Me, New sc.PropertyChangedEventArgs(propertyName))
      End Sub
      Protected Overrides Sub OnPropertyChanged(propertyName As String)
         OnExtendedPropertyChanged(propertyName)
         MyBase.OnPropertyChanged(propertyName)
      End Sub
      Protected Overrides Sub OnPropertyChanged(Of TProperty)(propertyName As String, newValue As TProperty, oldValue As TProperty)
         OnExtendedPropertyChanged(Of TProperty)(propertyName, newValue, oldValue)
         MyBase.OnPropertyChanged(Of TProperty)(propertyName, newValue, oldValue)
      End Sub
      Protected Sub OnExtendedPropertyChanged(Of TProperty)(propertyName As String, newValue As TProperty, oldValue As TProperty)
         RaiseEvent ExtendedPropertyChanged(Me, propertyName.ToPropertyChangedEventArgs(newValue, oldValue))
      End Sub
#End Region

#Region "IComparable"
      Public Shared Function Compare(first As cmisProperty, second As cmisProperty) As Integer
         If first Is second Then
            Return 0
         ElseIf first Is Nothing Then
            Return -1
         ElseIf second Is Nothing Then
            Return 1
         Else
            Return first.CompareTo(second)
         End If
      End Function
      Protected MustOverride Function CompareTo(other As Object) As Integer Implements IComparable.CompareTo
      Protected MustOverride Function CompareTo(other As cmisProperty) As Integer Implements IComparable(Of cmisProperty).CompareTo
#End Region

      ''' <summary>
      ''' Cardinality returns the correct value if this instance of a cmisProperty is created by a cmisPropertyDefinitionType-instance.
      ''' If this instance has been created during deserialization the returned value is enumCardinality.multi (default) until the property
      ''' is changed by custom code.
      ''' </summary>
      Public Property Cardinality As enumCardinality = enumCardinality.multi

      Private Shared Sub OnDecimalRepresentationChanged(newValue As enumDecimalRepresentation)
         Dim attrs As Object() = GetType(cmisPropertyDecimal).GetCustomAttributes(GetType(cac), False)
         Dim attr As cac = If(attrs IsNot Nothing AndAlso attrs.Length > 0, CType(attrs(0), cac), Nothing)

         Select Case newValue
            Case enumDecimalRepresentation.decimal
               cac.AppendTypeFactory(Of cmisProperty)(_factories, _genericTypeDefinition, GetType(cmisPropertyDecimal), attr)
            Case enumDecimalRepresentation.double
               cac.AppendTypeFactory(Of cmisProperty)(_factories, _genericTypeDefinition, GetType(cmisPropertyDouble), attr)
         End Select
      End Sub

      Public MustOverride ReadOnly Property PropertyType As Type

      Protected _propertyDefinition As Definitions.Properties.cmisPropertyDefinitionType
      ''' <summary>
      ''' The cmisPropertyDefinitionType-instance created this cmisPropertyType
      ''' (the value is null if the current instance is created by deserialization)
      ''' </summary>
      Public Property PropertyDefinition As Definitions.Properties.cmisPropertyDefinitionType
         Get
            Return _propertyDefinition
         End Get
         Set(value As Definitions.Properties.cmisPropertyDefinitionType)
            _propertyDefinition = value
         End Set
      End Property

      Public MustOverride ReadOnly Property Type As Core.enumPropertyType

      Public Property Value As Object
         Get
            Return ValueCore
         End Get
         Set(value As Object)
            ValueCore = value
         End Set
      End Property
      Protected MustOverride Property ValueCore As Object
      ''' <summary>
      ''' Sets Value without raising the PropertyChanged-Event
      ''' </summary>
      ''' <param name="value"></param>
      ''' <remarks></remarks>
      Public MustOverride Function SetValueSilent(value As Object) As Object
      Public Property Values As Object()
         Get
            Return ValuesCore
         End Get
         Set(value As Object())
            ValuesCore = value
         End Set
      End Property
      Protected MustOverride Property ValuesCore As Object()
      ''' <summary>
      ''' Sets Values without raising the PropertyChanged-Event
      ''' </summary>
      ''' <param name="values"></param>
      ''' <remarks></remarks>
      Public MustOverride Function SetValuesSilent(values As Object()) As Object()

   End Class

   ''' <summary>
   ''' Base-class for all string containing properties avoids null values within property Values
   ''' </summary>
   ''' <remarks>Null values result in a failure when doing a query from the apache workbench</remarks>
   Public MustInherit Class cmisPropertyStringBase
      Inherits Generic.cmisProperty(Of String)

      Protected Sub New()
         MyBase.New()
      End Sub
      ''' <summary>
      ''' this constructor is only used if derived classes from this class needs an InitClass()-call
      ''' </summary>
      ''' <param name="initClassSupported"></param>
      ''' <remarks></remarks>
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub
      Protected Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String, ParamArray values As String())
         MyBase.New(propertyDefinitionId, localName, displayName, queryName)
         _values = values
      End Sub

      Public Overrides Property Values As String()
         Get
            Return MyBase.Values
         End Get
         Set(value As String())
            If value Is Nothing Then
               MyBase.Values = value
            Else
               MyBase.Values = (From currentValue As String In value Select If(currentValue, String.Empty)).ToArray()
            End If
         End Set
      End Property

   End Class

   Namespace Generic
      ''' <summary>
      ''' Generic version of cmisProperty-class
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <remarks>Baseclass of all typesafe cmisProperty-classes</remarks>
      Public MustInherit Class cmisProperty(Of TProperty)
         Inherits cmisProperty
         Implements IComparable(Of cmisProperty(Of TProperty))

         Protected Sub New()
            MyBase.New()
         End Sub
         ''' <summary>
         ''' this constructor is only used if derived classes from this class needs an InitClass()-call
         ''' </summary>
         ''' <param name="initClassSupported"></param>
         ''' <remarks></remarks>
         Protected Sub New(initClassSupported As Boolean?)
            MyBase.New(initClassSupported)
         End Sub
         Protected Sub New(propertyDefinitionId As String, localName As String, displayName As String, queryName As String, ParamArray values As TProperty())
            MyBase.New(propertyDefinitionId, localName, displayName, queryName)
            _values = values
         End Sub

#Region "IComparable"
         ''' <summary>
         ''' Compares the content of the current instance with the content of the other-instance
         ''' </summary>
         ''' <param name="other"></param>
         ''' <returns></returns>
         ''' <remarks>comparisons only defined for following other-types:
         ''' cmisProperty(Of TProperty), TProperty, IEnumerable(Of TProperty)</remarks>
         Protected Overrides Function CompareTo(other As Object) As Integer
            If other Is Nothing Then
               Return 1
            ElseIf TypeOf other Is cmisProperty(Of TProperty) Then
               Return CompareTo(CType(other, cmisProperty(Of TProperty)))
            ElseIf TypeOf other Is TProperty Then
               Return CompareTo(CType(other, TProperty))
            ElseIf TypeOf other Is IEnumerable(Of TProperty) Then
               Return CompareTo(CType(other, IEnumerable(Of TProperty)).ToArray())
            Else
               'unable to compare
               Return 0
            End If
         End Function
         ''' <summary>
         ''' Compares the content of the current instance with the content of the other-instance
         ''' </summary>
         ''' <param name="other"></param>
         ''' <returns></returns>
         ''' <remarks>comparisons only defined for other-objects of type cmisProperty(Of TProperty)</remarks>
         Protected Overrides Function CompareTo(other As cmisProperty) As Integer
            If TypeOf other Is cmisProperty(Of TProperty) Then
               Return CompareTo(CType(other, cmisProperty(Of TProperty)))
            Else
               'unable to compare
               Return 0
            End If
         End Function
         Protected Overloads Function CompareTo(other As cmisProperty(Of TProperty)) As Integer Implements IComparable(Of CmisObjectModel.Core.Properties.Generic.cmisProperty(Of TProperty)).CompareTo
            If other Is Nothing Then
               Return 1
            ElseIf other Is Me Then
               Return 0
            Else
               Return CompareTo(other._values)
            End If
         End Function
         Protected MustOverride Overloads Function CompareTo(ParamArray other As TProperty()) As Integer
#End Region

         ''' <summary>
         ''' Returns the runtimetype of the cmisProperty (enumCardinality.single).
         ''' Note: if this cmisProperty supports multiple entries (array), the
         ''' returned value is the elementType of the arrayType.
         ''' </summary>
         Public Overrides ReadOnly Property PropertyType As System.Type
            Get
               Return GetType(TProperty)
            End Get
         End Property

         Public Overridable Shadows Property Value As TProperty
            Get
               Return If(_values Is Nothing OrElse _values.Length = 0, Nothing, _values(0))
            End Get
            Set(value As TProperty)
               Dim oldValue As TProperty() = _values
               _values = New TProperty() {value}
               OnPropertyChanged("Values", _values, oldValue)
            End Set
         End Property
         Protected Overrides Property ValueCore As Object
            Get
               Return Value
            End Get
            Set(value As Object)
               Me.Value = CType(If(value Is Nothing OrElse TypeOf value Is TProperty,
                                   value, Common.TryCastDynamic(value, GetType(TProperty))), TProperty)
            End Set
         End Property
         ''' <summary>
         ''' Sets _values without raising the PropertyChanged-Event
         ''' </summary>
         ''' <param name="value"></param>
         ''' <remarks></remarks>
         Public Overrides Function SetValueSilent(value As Object) As Object
            Dim retVal As Object = Me.Value

            If TypeOf value Is TProperty OrElse value Is Nothing Then
               _values = New TProperty() {CType(value, TProperty)}
               OnExtendedPropertyChanged("Value", value, retVal)
            End If
            Return retVal
         End Function

         Protected _values As TProperty()
         Public Overridable Shadows Property Values As TProperty()
            Get
               Return _values
            End Get
            Set(value As TProperty())
               If value IsNot _values Then
                  Dim oldValue As TProperty() = _values
                  _values = value
                  OnPropertyChanged("Values", value, oldValue)
               End If
            End Set
         End Property 'Values
         Protected Overrides Property ValuesCore As Object()
            Get
               If _values Is Nothing Then
                  Return Nothing
               Else
                  Return (From value As Object In _values
                          Select value).ToArray()
               End If
            End Get
            Set(value As Object())
               If value Is Nothing Then
                  Values = Nothing
               Else
                  Values = (From rawItem As Object In value
                            Let item As Object = If(rawItem Is Nothing OrElse TypeOf rawItem Is TProperty,
                                                    rawItem, Common.TryCastDynamic(rawItem, GetType(TProperty)))
                            Select CType(item, TProperty)).ToArray()
               End If
            End Set
         End Property
         ''' <summary>
         ''' Sets _values without raising the PropertyChanged-Event
         ''' </summary>
         ''' <param name="values"></param>
         ''' <remarks></remarks>
         Public Overrides Function SetValuesSilent(values() As Object) As Object()
            Dim retVal As Object() = ValuesCore

            _values = (From rawValue As Object In values
                       Let value As Object = If(rawValue Is Nothing OrElse TypeOf rawValue Is TProperty,
                                                rawValue, Common.TryCastDynamic(rawValue, GetType(TProperty)))
                       Select CType(value, TProperty)).ToArray()
            OnExtendedPropertyChanged("Values", ValuesCore, retVal)
            Return retVal
         End Function

         Public Overrides Function ToString() As String
            Return GetType(TProperty).Name & "/" & _propertyDefinitionId & " = " & If(_values Is Nothing, "null", String.Join(", ", _values))
         End Function

      End Class
   End Namespace
End Namespace