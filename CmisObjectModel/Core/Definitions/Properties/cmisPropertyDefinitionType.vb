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
Imports ccg = CmisObjectModel.Collections.Generic
Imports ccc = CmisObjectModel.Core.Choices
Imports cccg = CmisObjectModel.Core.Choices.Generic
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdpg = CmisObjectModel.Core.Definitions.Properties.Generic
Imports ccp = CmisObjectModel.Core.Properties
Imports cjcdp = CmisObjectModel.JSON.Core.Definitions.Properties
Imports cjcdpg = CmisObjectModel.JSON.Core.Definitions.Properties.Generic
Imports cjs = CmisObjectModel.JSON.Serialization

Namespace CmisObjectModel.Core.Definitions.Properties
   <Attributes.JavaScriptObjectResolver(GetType(cjs.CmisPropertyDefinitionResolver)),
    Attributes.JavaScriptConverter(GetType(cjcdp.cmisPropertyDefinitionTypeConverter))>
   Partial Public Class cmisPropertyDefinitionType
      Implements Contracts.IPropertyDefinition

#Region "Constructors"
      Protected Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                        required As Boolean, inherited As Boolean, queryable As Boolean, orderable As Boolean,
                        cardinality As enumCardinality, updatability As enumUpdatability)
         MyBase.New(id, localName, localNamespace, displayName, queryName, queryable)
         Me.Required = required
         Me.Inherited = inherited
         Me.Cardinality = cardinality
         Me.Updatability = updatability
         Me.Orderable = orderable
      End Sub

      ''' <summary>
      ''' Creates a new instance suitable for the current node of the reader
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' Node "propertyType" is responsible for type of returned PropertyDefinition-instance
      ''' </remarks>
      Public Overloads Shared Function CreateInstance(reader As System.Xml.XmlReader) As cmisPropertyDefinitionType
         'first chance: from current node name
         Dim nodeName As String = GetCurrentStartElementLocalName(reader)
         If Not String.IsNullOrEmpty(nodeName) Then
            nodeName = nodeName.ToLowerInvariant()
            If _factories.ContainsKey(nodeName) Then
               Dim retVal As cmisPropertyDefinitionType = TryCast(_factories(nodeName).CreateInstance(), cmisPropertyDefinitionType)

               If retVal IsNot Nothing Then
                  retVal.ReadXml(reader)
                  Return retVal
               End If
            ElseIf nodeName = "cmisproperty" OrElse nodeName = "propertydefinition" OrElse nodeName = "cmispropertydefinitiontype" Then
               'second chance: child element named 'propertyType'
               Return CreateInstance(Of cmisPropertyDefinitionType)(reader, "propertyType")
            End If
         End If

         'unable to interpret node as cmisproperty
         Return Nothing
      End Function
#End Region

#Region "IPropertyDefinition mirrored properties"
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_Cardinality As enumCardinality Implements Contracts.IPropertyDefinition.Cardinality
         Get
            Return _cardinality
         End Get
         Set(value As enumCardinality)
            Me.Cardinality = value
         End Set
      End Property

      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_Inherited As Boolean? Implements Contracts.IPropertyDefinition.Inherited
         Get
            Return _inherited
         End Get
         Set(value As Boolean?)
            Me.Inherited = value
         End Set
      End Property

      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_OpenChoice As Boolean? Implements Contracts.IPropertyDefinition.OpenChoice
         Get
            Return _openChoice
         End Get
         Set(value As Boolean?)
            Me.OpenChoice = value
         End Set
      End Property

      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_Orderable As Boolean Implements Contracts.IPropertyDefinition.Orderable
         Get
            Return _orderable
         End Get
         Set(value As Boolean)
            Me.Orderable = value
         End Set
      End Property

      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_Required As Boolean Implements Contracts.IPropertyDefinition.Required
         Get
            Return _required
         End Get
         Set(value As Boolean)
            Me.Required = value
         End Set
      End Property

      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Private Property IPropertyDefinition_Updatability As enumUpdatability Implements Contracts.IPropertyDefinition.Updatability
         Get
            Return _updatability
         End Get
         Set(value As enumUpdatability)
            Me.Updatability = value
         End Set
      End Property
#End Region

#Region "IXmlSerialization"
      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         MyBase.ReadXml(reader)
         'support browser binding properties
         ExtendedProperties.Add(Constants.ExtendedProperties.Cardinality, _cardinality.GetName())
      End Sub

      Protected Overrides Sub OnPropertyChanged(e As System.ComponentModel.PropertyChangedEventArgs)
         If String.Compare(e.PropertyName, "Cardinality", True) = 0 Then
            Dim extendedProperties = Me.ExtendedProperties

            If extendedProperties.ContainsKey(Constants.ExtendedProperties.Cardinality) Then
               extendedProperties(Constants.ExtendedProperties.Cardinality) = _cardinality.GetName()
            Else
               extendedProperties.Add(Constants.ExtendedProperties.Cardinality, _cardinality.GetName())
            End If
         End If
         MyBase.OnPropertyChanged(e)
      End Sub
#End Region

      ''' <summary>
      ''' Creates a property-instance for this property-definition
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public MustOverride Function CreateProperty() As Core.Properties.cmisProperty Implements Contracts.IPropertyDefinition.CreateProperty
      Public MustOverride Function CreateProperty(ParamArray values As Object()) As Core.Properties.cmisProperty Implements Contracts.IPropertyDefinition.CreateProperty

      Public Property Choices As Core.Choices.cmisChoice() Implements Contracts.IPropertyDefinition.Choices
         Get
            Return ChoicesCore
         End Get
         Set(value As Core.Choices.cmisChoice())
            ChoicesCore = value
         End Set
      End Property
      Protected MustOverride Property ChoicesCore As Core.Choices.cmisChoice()

      Private _choicesAsReadOnly As New ccg.ArrayMapper(Of cmisPropertyDefinitionType, Choices.cmisChoice)(Me,
                                                                                                           "Choices", Function() ChoicesCore,
                                                                                                           "DisplayName", Function(choice) choice.DisplayName)
      ''' <summary>
      ''' Access to choices via index or DisplayName
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property ChoicesAsReadOnly As ccg.ArrayMapper(Of cmisPropertyDefinitionType, Choices.cmisChoice) Implements Contracts.IPropertyDefinition.ChoicesAsReadOnly
         Get
            Return _choicesAsReadOnly
         End Get
      End Property

      Public MustOverride ReadOnly Property ChoiceType As Type Implements Contracts.IPropertyDefinition.ChoiceType
      Public MustOverride ReadOnly Property CreatePropertyResultType As Type Implements Contracts.IPropertyDefinition.CreatePropertyResultType

      Public Property DefaultValue As Core.Properties.cmisProperty Implements Contracts.IPropertyDefinition.DefaultValue
         Get
            Return DefaultValueCore
         End Get
         Set(value As Core.Properties.cmisProperty)
            DefaultValueCore = value
         End Set
      End Property
      Protected MustOverride Property DefaultValueCore As Core.Properties.cmisProperty

      Public ReadOnly Property PropertyType As Core.enumPropertyType Implements Contracts.IPropertyDefinition.PropertyType
         Get
            Return _propertyType
         End Get
      End Property
      Protected MustOverride ReadOnly Property _propertyType As Core.enumPropertyType

      Public MustOverride ReadOnly Property PropertyValueType As Type Implements Contracts.IPropertyDefinition.PropertyValueType

   End Class

   Namespace Generic
      ''' <summary>
      ''' Generic version of cmisPropertyDefinitionType
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <typeparam name="TChoice"></typeparam>
      ''' <typeparam name="TDefaultValue"></typeparam>
      ''' <remarks>Baseclass of all typesafe cmisPropertyDefinitionType-classes</remarks>
      <Attributes.JavaScriptConverter(GetType(cjcdpg.cmisPropertyDefinitionTypeConverter(Of Boolean, ccc.cmisChoiceBoolean, 
                                                                                            ccp.cmisPropertyBoolean, 
                                                                                            ccdp.cmisPropertyBooleanDefinitionType)),
                                      "{"""":""TPropertyDefinition"",""TProperty"":""TProperty"",""TChoice"":""TChoice"",""TDefaultValue"":""TDefaultValue""}")>
      Public MustInherit Class cmisPropertyDefinitionType(Of TProperty, TChoice As {Core.Choices.Generic.cmisChoice(Of TProperty, TChoice), New},
                                                             TDefaultValue As {New, Core.Properties.Generic.cmisProperty(Of TProperty)})
         Inherits cmisPropertyDefinitionType

         Protected Sub New()
         End Sub
         ''' <summary>
         ''' this constructor is only used if derived classes from this class needs an InitClass()-call
         ''' </summary>
         ''' <param name="initClassSupported"></param>
         ''' <remarks></remarks>
         Protected Sub New(initClassSupported As Boolean?)
            MyBase.New(initClassSupported)
         End Sub
         Protected Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                           required As Boolean, inherited As Boolean, queryable As Boolean, orderable As Boolean,
                           cardinality As enumCardinality, updatability As enumUpdatability,
                           ParamArray choices As TChoice())
            MyBase.New(id, localName, localNamespace, displayName, queryName,
                       required, inherited, queryable, orderable, cardinality, updatability)
            _choices = choices
         End Sub
         Protected Sub New(id As String, localName As String, localNamespace As String, displayName As String, queryName As String,
                           required As Boolean, inherited As Boolean, queryable As Boolean, orderable As Boolean,
                           cardinality As enumCardinality, updatability As enumUpdatability, defaultValue As TDefaultValue,
                           ParamArray choices As TChoice())
            MyBase.New(id, localName, localNamespace, displayName, queryName,
                       required, inherited, queryable, orderable, cardinality, updatability)
            _defaultValue = defaultValue
            _choices = choices
         End Sub

         ''' <summary>
         ''' Creates a property-instance for this property definition
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overrides Function CreateProperty() As Core.Properties.cmisProperty
            Dim retVal As New TDefaultValue With {.Cardinality = Me.Cardinality, .DisplayName = Me.DisplayName, .LocalName = Me.LocalName,
                                                  .PropertyDefinition = Me, .PropertyDefinitionId = Me.Id, .QueryName = Me.QueryName}
            If _extendedProperties IsNot Nothing Then
               Dim extendedProperties As Dictionary(Of String, Object) = retVal.ExtendedProperties
               For Each de As KeyValuePair(Of String, Object) In _extendedProperties
                  If Not extendedProperties.ContainsKey(de.Key) Then extendedProperties.Add(de.Key, de.Value)
               Next
            End If
            Return retVal
         End Function
         Public Overrides Function CreateProperty(ParamArray values As Object()) As Core.Properties.cmisProperty
            If values Is Nothing OrElse values.Length = 0 Then
               Return Me.CreateProperty()
            Else
               Dim retVal As New TDefaultValue With {.Cardinality = Me.Cardinality, .DisplayName = Me.DisplayName, .LocalName = Me.LocalName,
                                                     .PropertyDefinition = Me, .PropertyDefinitionId = Me.Id, .QueryName = Me.QueryName,
                                                     .Values = (From value As Object In values
                                                                Where value Is Nothing OrElse TypeOf value Is TProperty
                                                                Select CType(value, TProperty)).ToArray}
               If _extendedProperties IsNot Nothing Then
                  Dim extendedProperties As Dictionary(Of String, Object) = retVal.ExtendedProperties
                  For Each de As KeyValuePair(Of String, Object) In _extendedProperties
                     If Not extendedProperties.ContainsKey(de.Key) Then extendedProperties.Add(de.Key, de.Value)
                  Next
               End If
               Return retVal
            End If
         End Function

         Protected _choices As TChoice()
         Public Overridable Shadows Property Choices As TChoice()
            Get
               Return _choices
            End Get
            Set(value As TChoice())
               If value IsNot _choices Then
                  Dim oldValue As TChoice() = _choices
                  _choices = value
                  OnPropertyChanged("Choices", value, oldValue)
               End If
            End Set
         End Property 'Choices
         Protected Overrides Property ChoicesCore As Core.Choices.cmisChoice()
            Get
               If _choices Is Nothing Then
                  Return Nothing
               Else
                  Return (From choice As Core.Choices.cmisChoice In _choices
                          Select choice).ToArray()
               End If
            End Get
            Set(value As Choices.cmisChoice())
               If value Is Nothing Then
                  Choices = Nothing
               Else
                  Choices = (From choice As Core.Choices.cmisChoice In value
                             Where TypeOf choice Is TChoice
                             Select CType(choice, TChoice)).ToArray()
               End If
            End Set
         End Property

         Private _choicesAsReadOnly As New ccg.ArrayMapper(Of cmisPropertyDefinitionType, TChoice)(Me,
                                                                                                   "Choices", Function() _choices,
                                                                                                   "DisplayName", Function(choice) choice.DisplayName)
         ''' <summary>
         ''' Access to choices via index or DisplayName
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Shadows ReadOnly Property ChoicesAsReadOnly As ccg.ArrayMapper(Of cmisPropertyDefinitionType, TChoice)
            Get
               Return _choicesAsReadOnly
            End Get
         End Property

         Public Overrides ReadOnly Property ChoiceType As System.Type
            Get
               Return GetType(TChoice)
            End Get
         End Property

         Public Overrides ReadOnly Property CreatePropertyResultType As System.Type
            Get
               Return GetType(TDefaultValue)
            End Get
         End Property

         Protected _defaultValue As TDefaultValue
         Public Overridable Shadows Property DefaultValue As TDefaultValue
            Get
               Return _defaultValue
            End Get
            Set(value As TDefaultValue)
               If value IsNot _defaultValue Then
                  Dim oldValue As TDefaultValue = _defaultValue
                  _defaultValue = value
                  OnPropertyChanged("DefaultValue", value, oldValue)
               End If
            End Set
         End Property 'DefaultValue
         Protected Overrides Property DefaultValueCore As Core.Properties.cmisProperty
            Get
               Return _defaultValue
            End Get
            Set(value As Core.Properties.cmisProperty)
               DefaultValue = If(TypeOf value Is TDefaultValue, CType(value, TDefaultValue), Nothing)
            End Set
         End Property

         Public Overrides ReadOnly Property PropertyValueType As System.Type
            Get
               Return GetType(TProperty)
            End Get
         End Property

         Public Overrides Function ToString() As String
            Return GetType(TProperty).Name & "/" & Id
         End Function

      End Class
   End Namespace
End Namespace