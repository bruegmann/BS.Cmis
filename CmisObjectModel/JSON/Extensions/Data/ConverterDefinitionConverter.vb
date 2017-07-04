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
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Extensions.Data
   ''' <summary>
   ''' Converter for ConverterDefinition-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ConverterDefinitionConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Extensions.Data.ConverterDefinition)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Extensions.Data.ConverterDefinition))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Extensions.Data.ConverterDefinition))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim items As CmisObjectModel.Extensions.Data.ConverterDefinitionItem() = Nothing

            .Object.ConverterIdentifier = Read(.Dictionary, "converterIdentifier", .Object.ConverterIdentifier)
            items = ReadArray(context, "items", items)
            If items IsNot Nothing Then
               For Each item As CmisObjectModel.Extensions.Data.ConverterDefinitionItem In items
                  .Object.Add(item)
               Next
            End If
            .Object.LocalType = ReadOptionalEnum(.Dictionary, "localType", .Object.LocalType)
            .Object.NullValueMapping = Read(.Dictionary, "nullValueMapping", .Object.NullValueMapping)
            .Object.PropertyDefinitionId = Read(.Dictionary, "propertyDefinitionId", .Object.PropertyDefinitionId)
            .Object.RemoteType = ReadOptionalEnum(.Dictionary, "remoteType", .Object.RemoteType)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            Dim items As CmisObjectModel.Extensions.Data.ConverterDefinitionItem() = .Object.Items

            .Add("converterIdentifier", .Object.ConverterIdentifier)
            .Add("extensionTypeName", .Object.ExtensionTypeName)
            If items IsNot Nothing AndAlso items.Length > 0 Then WriteArray(context, items, "items")
            If .Object.LocalType.HasValue Then .Add("localType", .Object.LocalType.Value.GetName())
            .Add("nullValueMapping", .Object.NullValueMapping)
            .Add("propertyDefinitionId", .Object.PropertyDefinitionId)
            If .Object.RemoteType.HasValue Then .Add("remoteType", .Object.RemoteType.Value.GetName())
         End With
      End Sub

   End Class
End Namespace