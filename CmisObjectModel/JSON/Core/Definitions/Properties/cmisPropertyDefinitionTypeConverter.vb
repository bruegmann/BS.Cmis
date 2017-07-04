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
Imports cccg = CmisObjectModel.Core.Choices.Generic
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdpg = CmisObjectModel.Core.Definitions.Properties.Generic
Imports ccpg = CmisObjectModel.Core.Properties.Generic
Imports cjs = CmisObjectModel.JSON.Serialization
Imports cjsg = CmisObjectModel.JSON.Serialization.Generic
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Definitions.Properties
   Partial Class cmisPropertyDefinitionType
      Public Shared ReadOnly DefaultKeyProperty As Common.Generic.DynamicProperty(Of cmisPropertyDefinitionType, String) =
         New Common.Generic.DynamicProperty(Of cmisPropertyDefinitionType, String)(Function(item) item._id,
                                                                                   Sub(item, value)
                                                                                      item.Id = value
                                                                                   End Sub, "Id")
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core.Definitions.Properties
   ''' <summary>
   ''' cmisPropertyDefinitionType-converter
   ''' </summary>
   ''' <remarks></remarks>
   Public NotInheritable Class cmisPropertyDefinitionTypeConverter
      Inherits Generic.cmisPropertyDefinitionTypeConverter(Of ccdp.cmisPropertyDefinitionType)

#Region "Constructors"
      Public Sub New()
         MyBase.New()
      End Sub
      Public Sub New(objectResolver As cjsg.ObjectResolver(Of ccdp.cmisPropertyDefinitionType))
         MyBase.New(objectResolver)
      End Sub
#End Region

   End Class

   ''' <summary>
   ''' Contract to access non generic methods across generic cmisPropertyDefinitionTypeConverter-types
   ''' </summary>
   ''' <remarks></remarks>
   <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
   Friend Interface IcmisPropertyDefinitionTypeConverter
      Sub Deserialize(retVal As ccdp.cmisPropertyDefinitionType, dictionary As IDictionary(Of String, Object), serializer As cjs.JavaScriptSerializer)
      Function Serialize(obj As ccdp.cmisPropertyDefinitionType, serializer As cjs.JavaScriptSerializer) As IDictionary(Of String, Object)
   End Interface
   Namespace Generic
      ''' <summary>
      ''' Baseclass of all cmisPropertyDefintionType-converters
      ''' </summary>
      ''' <remarks></remarks>
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public MustInherit Class cmisPropertyDefinitionTypeConverter(Of TPropertyDefinition As ccdp.cmisPropertyDefinitionType)
         Inherits cjsg.JavaScriptConverter(Of TPropertyDefinition, cjsg.ObjectResolver(Of ccdp.cmisPropertyDefinitionType))
         Implements IcmisPropertyDefinitionTypeConverter

#Region "Constructors"
         Public Sub New()
            MyBase.New(New cjs.CmisPropertyDefinitionResolver())
         End Sub
         Public Sub New(objectResolver As cjsg.ObjectResolver(Of ccdp.cmisPropertyDefinitionType))
            MyBase.New(objectResolver)
         End Sub
#End Region

         ''' <summary>
         ''' Converts dictionary to a XmlSerializable-instance of type SupportedType
         ''' </summary>
         ''' <param name="serializer">MUST be a CmisObjectModelLibrary.JSON.JavaScriptSerializer</param>
         ''' <param name="type">ignored, should be nothing</param>
         Public NotOverridable Overrides Function Deserialize(dictionary As System.Collections.Generic.IDictionary(Of String, Object),
                                                              type As System.Type, serializer As swss.JavaScriptSerializer) As Object
            If dictionary Is Nothing Then
               Return Nothing
            Else
               Dim retVal As ccdp.cmisPropertyDefinitionType = _objectResolver.CreateInstance(dictionary)
               If retVal IsNot Nothing Then
                  With CType(serializer, cjs.JavaScriptSerializer)
                     Dim converter As IcmisPropertyDefinitionTypeConverter =
                        If(TryCast(.GetJavaScriptConverter(retVal.GetType()), IcmisPropertyDefinitionTypeConverter), Me)
                     converter.Deserialize(retVal, dictionary, .Self)
                  End With
               End If
               Return retVal
            End If
         End Function
         Protected Overridable Overloads Sub Deserialize(retVal As ccdp.cmisPropertyDefinitionType, dictionary As IDictionary(Of String, Object), serializer As cjs.JavaScriptSerializer) Implements IcmisPropertyDefinitionTypeConverter.Deserialize
            Deserialize(New SerializationContext(CType(retVal, TPropertyDefinition), dictionary, serializer))
         End Sub
         Protected Overridable Overloads Sub Deserialize(context As SerializationContext)
            With context
               .Object.Id = Read(.Dictionary, "id", .Object.Id)
               .Object.LocalName = Read(.Dictionary, "localName", .Object.LocalName)
               .Object.LocalNamespace = Read(.Dictionary, "localNamespace", .Object.LocalNamespace)
               .Object.DisplayName = Read(.Dictionary, "displayName", .Object.DisplayName)
               .Object.QueryName = Read(.Dictionary, "queryName", .Object.QueryName)
               .Object.Description = Read(.Dictionary, "description", .Object.Description)
               'propertyType is readOnly
               .Object.Cardinality = ReadEnum(.Dictionary, "cardinality", .Object.Cardinality)
               .Object.Updatability = ReadEnum(.Dictionary, "updatability", .Object.Updatability)
               .Object.Inherited = ReadNullable(.Dictionary, "inherited", .Object.Inherited)
               .Object.Required = Read(.Dictionary, "required", .Object.Required)
               .Object.Queryable = Read(.Dictionary, "queryable", .Object.Queryable)
               .Object.Orderable = Read(.Dictionary, "orderable", .Object.Orderable)
               .Object.OpenChoice = ReadNullable(.Dictionary, "openChoice", .Object.OpenChoice)
            End With
         End Sub

         ''' <summary>
         ''' Converts XmlSerializable-instance to IDictionary(Of String, Object)
         ''' </summary>
         ''' <param name="serializer">MUST be a CmisObjectModelLibrary.JSON.JavaScriptSerializer</param>
         Public NotOverridable Overrides Function Serialize(obj As Object, serializer As swss.JavaScriptSerializer) As System.Collections.Generic.IDictionary(Of String, Object)
            If obj Is Nothing Then
               Return Nothing
            Else
               With CType(serializer, cjs.JavaScriptSerializer)
                  Dim converter As IcmisPropertyDefinitionTypeConverter =
                        If(TryCast(.GetJavaScriptConverter(obj.GetType()), IcmisPropertyDefinitionTypeConverter), Me)
                  Return converter.Serialize(CType(obj, ccdp.cmisPropertyDefinitionType), .Self)
               End With
            End If
         End Function
         Protected Overridable Overloads Function Serialize(obj As ccdp.cmisPropertyDefinitionType, serializer As cjs.JavaScriptSerializer) As IDictionary(Of String, Object) Implements IcmisPropertyDefinitionTypeConverter.Serialize
            Dim retVal As New Dictionary(Of String, Object)
            Serialize(New SerializationContext(CType(obj, TPropertyDefinition), retVal, serializer))
            Return retVal
         End Function
         Protected Overridable Overloads Sub Serialize(context As SerializationContext)
            With context
               .Add("id", .Object.Id)
               .Add("localName", .Object.LocalName)
               If Not String.IsNullOrEmpty(.Object.LocalNamespace) Then .Add("localNamespace", .Object.LocalNamespace)
               If Not String.IsNullOrEmpty(.Object.DisplayName) Then .Add("displayName", .Object.DisplayName)
               If Not String.IsNullOrEmpty(.Object.QueryName) Then .Add("queryName", .Object.QueryName)
               If Not String.IsNullOrEmpty(.Object.Description) Then .Add("description", .Object.Description)
               .Add("propertyType", .Object.PropertyType.GetName())
               .Add("cardinality", .Object.Cardinality.GetName())
               .Add("updatability", .Object.Updatability.GetName())
               .Add("inherited", .Object.Inherited.HasValue AndAlso .Object.Inherited.Value)
               .Add("required", .Object.Required)
               .Add("queryable", .Object.Queryable)
               .Add("orderable", .Object.Orderable)
               If .Object.OpenChoice.HasValue Then .Add("openChoice", .Object.OpenChoice.Value)
            End With
         End Sub
      End Class

      ''' <summary>
      ''' Baseclass of all converters for types derived from cmisPropertyDefintionType
      ''' </summary>
      ''' <typeparam name="TProperty"></typeparam>
      ''' <typeparam name="TChoice"></typeparam>
      ''' <typeparam name="TDefaultValue"></typeparam>
      ''' <typeparam name="TPropertyDefinition"></typeparam>
      ''' <remarks></remarks>
      Public Class cmisPropertyDefinitionTypeConverter(Of TProperty, TChoice As {cccg.cmisChoice(Of TProperty, TChoice), New},
                                                          TDefaultValue As {New, ccpg.cmisProperty(Of TProperty)},
                                                          TPropertyDefinition As ccdpg.cmisPropertyDefinitionType(Of TProperty, TChoice, TDefaultValue))
         Inherits cmisPropertyDefinitionTypeConverter(Of TPropertyDefinition)

#Region "Constructors"
         Public Sub New()
            MyBase.New()
         End Sub
         Public Sub New(objectResolver As cjsg.ObjectResolver(Of ccdp.cmisPropertyDefinitionType))
            MyBase.New(objectResolver)
         End Sub
#End Region

         Protected Overrides Sub Deserialize(context As SerializationContext)
            MyBase.Deserialize(context)
            With context
               If .Dictionary.ContainsKey("defaultValue") Then
                  Dim defaultValue As Object = .Dictionary("defaultValue")

                  .Object.DefaultValue = CType(.Object.CreateProperty(), TDefaultValue)
                  If TypeOf defaultValue Is ICollection Then
                     .Object.DefaultValue.Values = (From item As Object In CType(defaultValue, ICollection)
                                                    Select Common.TryCastDynamic(Of TProperty)(item)).ToArray()
                  Else
                     .Object.DefaultValue.Value = Common.TryCastDynamic(Of TProperty)(defaultValue)
                  End If
               End If
               .Object.Choices = ReadArray(context, "choice", .Object.Choices)
            End With
         End Sub

         Protected Overrides Sub Serialize(context As SerializationContext)
            MyBase.Serialize(context)
            With context
               If .Object.DefaultValue IsNot Nothing Then
                  Dim values As TProperty() = .Object.DefaultValue.Values

                  If values IsNot Nothing Then
                     If values.Length = 1 Then
                        .Add("defaultValue", values(0))
                     Else
                        .Add("defaultValue", values)
                     End If
                  End If
               End If
               If .Object.Choices IsNot Nothing Then WriteArray(context, .Object.Choices, "choice")
            End With
         End Sub
      End Class
   End Namespace
End Namespace