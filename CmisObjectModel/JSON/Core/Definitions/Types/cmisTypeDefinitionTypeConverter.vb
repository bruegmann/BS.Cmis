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
Imports ccdt = CmisObjectModel.Core.Definitions.Types
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

Namespace CmisObjectModel.Core.Definitions.Types
   Partial Class cmisTypeDefinitionType
      Public ReadOnly Property DefaultArrayProperty As Common.Generic.DynamicProperty(Of ccdp.cmisPropertyDefinitionType())
         Get
            Return New Common.Generic.DynamicProperty(Of ccdp.cmisPropertyDefinitionType())(Function() _propertyDefinitions,
                                                                                            Sub(value)
                                                                                               PropertyDefinitions = value
                                                                                            End Sub, "PropertyDefinitions")

         End Get
      End Property
   End Class

   <Attributes.JavaScriptConverter(GetType(JSON.Core.Definitions.Types.cmisTypeDocumentDefinitionTypeConverter))>
   Partial Class cmisTypeDocumentDefinitionType
   End Class


   <Attributes.JavaScriptConverter(GetType(JSON.Core.Definitions.Types.cmisTypeRelationshipDefinitionTypeConverter))>
   Partial Class cmisTypeRelationshipDefinitionType
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core.Definitions.Types
   ''' <summary>
   ''' Converter for cmisTypeDocumentDefinitionType
   ''' </summary>
   ''' <remarks></remarks>
   Public Class cmisTypeDocumentDefinitionTypeConverter
      Inherits Generic.cmisTypeDefinitionTypeConverter(Of ccdt.cmisTypeDocumentDefinitionType)

#Region "Constructors"
      Public Sub New()
         MyBase.New()
      End Sub
      Public Sub New(objectObserver As cjsg.ObjectResolver(Of ccdt.cmisTypeDefinitionType))
         MyBase.New(objectObserver)
      End Sub
#End Region

      Protected Overrides Sub Deserialize(context As SerializationContext)
         MyBase.Deserialize(context)
         With context
            .Object.Versionable = Read(.Dictionary, "versionable", .Object.Versionable)
            .Object.ContentStreamAllowed = ReadEnum(.Dictionary, "contentStreamAllowed", .Object.ContentStreamAllowed)
         End With
      End Sub

      Protected Overrides Sub Serialize(context As SerializationContext)
         MyBase.Serialize(context)
         With context
            .Add("versionable", .Object.Versionable)
            .Add("contentStreamAllowed", .Object.ContentStreamAllowed.GetName())
         End With
      End Sub
   End Class

   Public Class cmisTypeRelationshipDefinitionTypeConverter
      Inherits Generic.cmisTypeDefinitionTypeConverter(Of ccdt.cmisTypeRelationshipDefinitionType)

#Region "Constructors"
      Public Sub New()
         MyBase.New()
      End Sub
      Public Sub New(objectObserver As cjsg.ObjectResolver(Of ccdt.cmisTypeDefinitionType))
         MyBase.New(objectObserver)
      End Sub
#End Region

      Protected Overrides Sub Deserialize(context As SerializationContext)
         MyBase.Deserialize(context)
         With context
            .Object.AllowedSourceTypes = ReadArray(.Dictionary, "allowedSourceTypes", .Object.AllowedSourceTypes)
            .Object.AllowedTargetTypes = ReadArray(.Dictionary, "allowedTargetTypes", .Object.AllowedTargetTypes)
         End With
      End Sub

      Protected Overrides Sub Serialize(context As SerializationContext)
         MyBase.Serialize(context)
         With context
            Dim emptyArray As String() = New String() {}
            WriteArray(context, If(.Object.AllowedSourceTypes, emptyArray), "allowedSourceTypes")
            WriteArray(context, If(.Object.AllowedTargetTypes, emptyArray), "allowedTargetTypes")
         End With
      End Sub
   End Class

   ''' <summary>
   ''' Contract to access non generic methods across generic cmisTypeDefinitionTypeConverter-types
   ''' </summary>
   ''' <remarks></remarks>
   <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
   Friend Interface IcmisTypeDefinitionTypeConverter
      Sub Deserialize(retVal As ccdt.cmisTypeDefinitionType, dictionary As IDictionary(Of String, Object), serializer As cjs.JavaScriptSerializer)
      Function Serialize(obj As ccdt.cmisTypeDefinitionType, serializer As cjs.JavaScriptSerializer) As IDictionary(Of String, Object)
   End Interface
   Namespace Generic
      ''' <summary>
      ''' Base class of all cmisTypeDefinitionType converter-types
      ''' </summary>
      ''' <typeparam name="TTypeDefinition"></typeparam>
      ''' <remarks></remarks>
      Public Class cmisTypeDefinitionTypeConverter(Of TTypeDefinition As ccdt.cmisTypeDefinitionType)
         Inherits cjsg.JavaScriptConverter(Of TTypeDefinition, cjsg.ObjectResolver(Of ccdt.cmisTypeDefinitionType))
         Implements IcmisTypeDefinitionTypeConverter

#Region "Constructors"
         Public Sub New()
            MyBase.New(New cjs.CmisTypeDefinitionResolver())
         End Sub
         Public Sub New(objectResolver As cjsg.ObjectResolver(Of ccdt.cmisTypeDefinitionType))
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
               Dim retVal As ccdt.cmisTypeDefinitionType = _objectResolver.CreateInstance(dictionary)
               If retVal IsNot Nothing Then
                  With CType(serializer, cjs.JavaScriptSerializer)
                     Dim converter As IcmisTypeDefinitionTypeConverter =
                        If(TryCast(.GetJavaScriptConverter(retVal.GetType()), IcmisTypeDefinitionTypeConverter), Me)
                     converter.Deserialize(retVal, dictionary, .Self)
                  End With
               End If
               Return retVal
            End If
         End Function
         Protected Overridable Overloads Sub Deserialize(retVal As ccdt.cmisTypeDefinitionType, dictionary As IDictionary(Of String, Object), serializer As cjs.JavaScriptSerializer) Implements IcmisTypeDefinitionTypeConverter.Deserialize
            Deserialize(New SerializationContext(CType(retVal, TTypeDefinition), dictionary, serializer))
         End Sub
         Protected Overridable Overloads Sub Deserialize(context As SerializationContext)
            With context
               .Object.Id = Read(.Dictionary, "id", .Object.Id)
               .Object.LocalName = Read(.Dictionary, "localName", .Object.LocalName)
               .Object.DisplayName = Read(.Dictionary, "displayName", .Object.DisplayName)
               .Object.QueryName = Read(.Dictionary, "queryName", .Object.QueryName)
               .Object.Description = Read(.Dictionary, "description", .Object.Description)
               'baseId is readonly
               .Object.ParentId = Read(.Dictionary, "parentId", .Object.ParentId)
               .Object.Creatable = Read(.Dictionary, "creatable", .Object.Creatable)
               .Object.Fileable = Read(.Dictionary, "fileable", .Object.Fileable)
               .Object.Queryable = Read(.Dictionary, "queryable", .Object.Queryable)
               .Object.FulltextIndexed = Read(.Dictionary, "fulltextIndexed", .Object.FulltextIndexed)
               .Object.IncludedInSupertypeQuery = Read(.Dictionary, "includedInSupertypeQuery", .Object.IncludedInSupertypeQuery)
               .Object.ControllablePolicy = Read(.Dictionary, "controllablePolicy", .Object.ControllablePolicy)
               .Object.ControllableACL = Read(.Dictionary, "controllableACL", .Object.ControllableACL)
               .Object.TypeMutability = Read(context, "typeMutability", .Object.TypeMutability)
               If .Dictionary.ContainsKey("propertyDefinitions") Then
                  Dim mapper As New CmisObjectModel.Collections.Generic.ArrayMapper(Of ccdt.cmisTypeDefinitionType, ccdp.cmisPropertyDefinitionType, String)(
                     .Object, .Object.DefaultArrayProperty, ccdp.cmisPropertyDefinitionType.DefaultKeyProperty)
                  mapper.JavaImport(.Dictionary("propertyDefinitions"), context.Serializer)
               End If
               .Object.Extensions = ReadArray(context, "extensions", .Object.Extensions)
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
                  Dim converter As IcmisTypeDefinitionTypeConverter =
                     If(TryCast(.GetJavaScriptConverter(obj.GetType()), IcmisTypeDefinitionTypeConverter), Me)
                  Return converter.Serialize(CType(obj, ccdt.cmisTypeDefinitionType), .Self)
               End With
            End If
         End Function
         Protected Overridable Overloads Function Serialize(obj As ccdt.cmisTypeDefinitionType, serializer As cjs.JavaScriptSerializer) As IDictionary(Of String, Object) Implements IcmisTypeDefinitionTypeConverter.Serialize
            Dim retVal As New Dictionary(Of String, Object)
            Serialize(New SerializationContext(CType(obj, TTypeDefinition), retVal, serializer))
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
               .Add("baseId", .Object.BaseId.GetName())
               If Not String.IsNullOrEmpty(.Object.ParentId) Then .Add("parentId", .Object.ParentId)
               .Add("creatable", .Object.Creatable)
               .Add("fileable", .Object.Fileable)
               .Add("queryable", .Object.Queryable)
               .Add("fulltextIndexed", .Object.FulltextIndexed)
               .Add("includedInSupertypeQuery", .Object.IncludedInSupertypeQuery)
               .Add("controllablePolicy", .Object.ControllablePolicy)
               .Add("controllableACL", .Object.ControllableACL)
               If .Object.TypeMutability IsNot Nothing Then Write(context, .Object.TypeMutability, "typeMutability")
               Dim mapper As New CmisObjectModel.Collections.Generic.ArrayMapper(Of ccdt.cmisTypeDefinitionType, ccdp.cmisPropertyDefinitionType, String)(
                  .Object, .Object.DefaultArrayProperty, ccdp.cmisPropertyDefinitionType.DefaultKeyProperty)
               .Add("propertyDefinitions", mapper.JavaExport(Nothing, context.Serializer))
               If .Object.Extensions IsNot Nothing Then WriteArray(context, .Object.Extensions, "extensions")
            End With
         End Sub
      End Class
   End Namespace
End Namespace