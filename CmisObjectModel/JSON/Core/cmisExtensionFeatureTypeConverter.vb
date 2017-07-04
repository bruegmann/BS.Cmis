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
Imports cc = CmisObjectModel.Core
Imports ccg = CmisObjectModel.Collections.Generic
Imports ccg1 = CmisObjectModel.Common.Generic
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core
   Partial Class cmisExtensionFeatureKeyValuePair
      Public Shared DefaultKeyProperty As New CmisObjectModel.Common.Generic.DynamicProperty(Of cc.cmisExtensionFeatureKeyValuePair, String)(
         Function(item) item._key,
         Sub(item, value)
            item.Key = value
         End Sub, "Key")

      Public Shared DefaultValueProperty As New CmisObjectModel.Common.Generic.DynamicProperty(Of cc.cmisExtensionFeatureKeyValuePair, String)(
         Function(item) item._value,
         Sub(item, value)
            item.Value = value
         End Sub, "Value")
   End Class

   <Attributes.JavaScriptConverter(GetType(JSON.Core.cmisExtensionFeatureTypeConverter))>
   Partial Class cmisExtensionFeatureType
      Public ReadOnly Property DefaultArrayProperty As ccg1.DynamicProperty(Of cc.cmisExtensionFeatureKeyValuePair())
         Get
            Return New ccg1.DynamicProperty(Of cc.cmisExtensionFeatureKeyValuePair())(Function() _featureDatas,
                                                                                      Sub(value)
                                                                                         FeatureDatas = value
                                                                                      End Sub, "FeatureDatas")
         End Get
      End Property
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core
   Public Class cmisExtensionFeatureTypeConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of cc.cmisExtensionFeatureType)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New Serialization.Generic.DefaultObjectResolver(Of cc.cmisExtensionFeatureType))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of cc.cmisExtensionFeatureType))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            .Object.CommonName = Read(.Dictionary, "commonName", .Object.CommonName)
            .Object.Description = Read(.Dictionary, "description", .Object.Description)
            If .Dictionary.ContainsKey("featureData") Then CreateFeaturesMap(.Object).JavaImport(.Dictionary("featureData"), .Serializer)
            .Object.Id = Read(.Dictionary, "id", .Object.Id)
            .Object.Url = Read(.Dictionary, "url", .Object.Url)
            .Object.VersionLabel = Read(.Dictionary, "versionLabel", .Object.VersionLabel)
         End With
      End Sub

      ''' <summary>
      ''' Creates an ArrayMapper for the featureDatas-property
      ''' </summary>
      Private Function CreateFeaturesMap(features As cc.cmisExtensionFeatureType) As ccg.ArrayMapper(Of cc.cmisExtensionFeatureType, 
                                                                                                        cc.cmisExtensionFeatureKeyValuePair,
                                                                                                        String, String)
         Return New ccg.ArrayMapper(Of cc.cmisExtensionFeatureType, 
                                       cc.cmisExtensionFeatureKeyValuePair,
                                       String, String)(features, features.DefaultArrayProperty,
                                                       cc.cmisExtensionFeatureKeyValuePair.DefaultKeyProperty,
                                                       cc.cmisExtensionFeatureKeyValuePair.DefaultValueProperty)
      End Function

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            If Not String.IsNullOrEmpty(.Object.CommonName) Then .Add("commonName", .Object.CommonName)
            If Not String.IsNullOrEmpty(.Object.Description) Then .Add("description", .Object.Description)
            If .Object.FeatureDatas IsNot Nothing Then .Add("featureData", CreateFeaturesMap(.Object).JavaExport(Nothing, .Serializer))
            .Add("id", .Object.Id)
            If Not String.IsNullOrEmpty(.Object.Url) Then .Add("url", .Object.Url)
            If Not String.IsNullOrEmpty(.Object.VersionLabel) Then .Add("versionLabel", .Object.VersionLabel)
         End With
      End Sub
   End Class
End Namespace