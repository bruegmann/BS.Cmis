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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Core.Properties
   Public MustInherit Class PropertyConverter
      Inherits JSON.Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Core.Properties.cmisProperty)

#Region "Constructors"
      Protected Sub New()
         MyBase.New(New JSON.Serialization.CmisPropertyResolver())
      End Sub
      Protected Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.Properties.cmisProperty))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            .Object.PropertyDefinitionId = Read(.Dictionary, "propertyDefinitionId", .Object.PropertyDefinitionId)
            'type is readonly
            .Object.LocalName = Read(.Dictionary, "localName", .Object.LocalName)
            .Object.DisplayName = Read(.Dictionary, "displayName", .Object.DisplayName)
            .Object.QueryName = Read(.Dictionary, "queryName", .Object.QueryName)

            Dim value As Object = Nothing
            Dim containsKey As Boolean = .Dictionary.TryGetValue("value", value)

            If TypeOf .Object Is CmisObjectModel.Core.Properties.cmisPropertyDateTime Then
               Dim [object] As CmisObjectModel.Core.Properties.cmisPropertyDateTime = CType(.Object, CmisObjectModel.Core.Properties.cmisPropertyDateTime)
               If Not containsKey OrElse value Is Nothing Then
                  [object].Values = Nothing
               ElseIf TypeOf value Is ICollection Then
                  [object].Values = ReadArray(.Dictionary, "value", [object].Values)
               Else
                  [object].Value = Read(.Dictionary, "value", [object].Value)
               End If
            ElseIf Not containsKey OrElse value Is Nothing Then
               .Object.Values = Nothing
            ElseIf TypeOf value Is ICollection Then
               .Object.Values = ReadArray(.Dictionary, "value", .Object.Values)
            Else
               .Object.Value = Read(.Dictionary, "value", .Object.Value)
            End If
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            .Add("propertyDefinitionId", .Object.PropertyDefinitionId)
            .Add("type", .Object.Type.GetName())
            If Not String.IsNullOrEmpty(.Object.LocalName) Then .Add("localName", .Object.LocalName)
            If Not String.IsNullOrEmpty(.Object.DisplayName) Then .Add("displayName", .Object.DisplayName)
            If Not String.IsNullOrEmpty(.Object.QueryName) Then .Add("queryName", .Object.QueryName)
            .Add("value", SerializeValue(context))
         End With
      End Sub

      Protected MustOverride Function SerializeValue(context As SerializationContext) As Object
   End Class
End Namespace