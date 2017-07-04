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

Namespace CmisObjectModel.Core.Properties
   <Attributes.JavaScriptConverter(GetType(JSON.Core.Properties.cmisPropertyConverter))>
   Partial Class cmisProperty
      Public Shared DefaultKeyProperty As New CmisObjectModel.Common.Generic.DynamicProperty(Of cmisProperty, String)(
         Function(item) item._propertyDefinitionId,
         Sub(item, value)
            item.PropertyDefinitionId = value
         End Sub, "PropertyDefinitionId")
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core.Properties
   ''' <summary>
   ''' Defaultconverter for a cmisProperty-instance
   ''' </summary>
   ''' <remarks></remarks>
   Public Class cmisPropertyConverter
      Inherits PropertyConverter

#Region "Constructors"
      Public Sub New()
         MyBase.New()
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Core.Properties.cmisProperty))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         MyBase.Deserialize(context)
         With context
            .Object.Cardinality = ReadEnum(.Dictionary, "cardinality", .Object.Cardinality)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         MyBase.Serialize(context)
         With context
            .Add("cardinality", .Object.Cardinality.GetName())
         End With
      End Sub

      Protected Overrides Function SerializeValue(context As SerializationContext) As Object
         With context
            If TypeOf .Object Is CmisObjectModel.Core.Properties.cmisPropertyDateTime Then
               Dim [object] As CmisObjectModel.Core.Properties.cmisPropertyDateTime = CType(.Object, CmisObjectModel.Core.Properties.cmisPropertyDateTime)

               If [object].Values Is Nothing Then
                  Return Nothing
               ElseIf .Object.Cardinality = CmisObjectModel.Core.enumCardinality.multi Then
                  Return (From value As DateTimeOffset In [object].Values
                          Select Common.ToJSONTime(value.DateTime)).ToArray()
               Else
                  Return Common.ToJSONTime([object].Value.DateTime)
               End If
            ElseIf .Object.Cardinality = CmisObjectModel.Core.enumCardinality.multi Then
               Return .Object.Values
            Else
               Return .Object.Value
            End If
         End With
      End Function
   End Class
End Namespace