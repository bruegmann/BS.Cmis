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

Namespace CmisObjectModel.Messaging
   <Attributes.JavaScriptConverter(GetType(JSON.Messaging.QueryResultListConverter))>
   Partial Class cmisObjectListType
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Messaging
   ''' <summary>
   ''' Converter for a cmisObjectListType-instance
   ''' </summary>
   ''' <remarks></remarks>
   Public Class QueryResultListConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Messaging.cmisObjectListType)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Messaging.cmisObjectListType))
      End Sub
      Public Sub New(objectResolver As JSON.Serialization.Generic.ObjectResolver(Of CmisObjectModel.Messaging.cmisObjectListType))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            .Object.Objects = ReadArray(context, "results", .Object.Objects)
            .Object.HasMoreItems = Read(.Dictionary, "hasMoreItems", .Object.HasMoreItems)
            .Object.NumItems = ReadNullable(.Dictionary, "numItems", .Object.NumItems)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            WriteArray(context, If(.Object.Objects, New CmisObjectModel.Core.cmisObjectType() {}), "results")
            .Add("hasMoreItems", .Object.HasMoreItems)
            If .Object.NumItems.HasValue Then .Add("numItems", .Object.NumItems.Value)
         End With
      End Sub
   End Class
End Namespace