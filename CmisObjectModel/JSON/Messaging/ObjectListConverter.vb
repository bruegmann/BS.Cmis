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

Namespace CmisObjectModel.Messaging.Responses
   <Attributes.JavaScriptConverter(GetType(JSON.Messaging.ObjectListConverter))>
   Partial Class getContentChangesResponse
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Messaging
   ''' <summary>
   ''' Converter for a cmisObjectListType expanded with changeLogToken-property
   ''' </summary>
   ''' <remarks>BrowserBinding uses for object-lists (except query result lists) an additional property changeLogToken
   ''' to use the http://docs.oasis-open.org/ns/cmis/browser/201103/objectList specification for all methods returning
   ''' object-lists (the property is only used for getContentChanges())</remarks>
   Public Class ObjectListConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Messaging.Responses.getContentChangesResponse)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Messaging.Responses.getContentChangesResponse))
      End Sub
      Public Sub New(objectObserver As JSON.Serialization.Generic.ObjectResolver(Of CmisObjectModel.Messaging.Responses.getContentChangesResponse))
         MyBase.New(objectObserver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim objects As New CmisObjectModel.Messaging.cmisObjectListType()

            .Object.Objects = objects
            objects.Objects = ReadArray(context, "objects", objects.Objects)
            objects.HasMoreItems = Read(.Dictionary, "hasMoreItems", objects.HasMoreItems)
            objects.NumItems = ReadNullable(.Dictionary, "numItems", objects.NumItems)
            .Object.ChangeLogToken = Read(.Dictionary, "changeLogToken", .Object.ChangeLogToken)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            Dim objects As CmisObjectModel.Core.cmisObjectType() = If(If(.Object.Objects Is Nothing, Nothing, .Object.Objects.Objects), New CmisObjectModel.Core.cmisObjectType() {})
            Dim hasMoreItems As Boolean = If(.Object.Objects Is Nothing, False, .Object.Objects.HasMoreItems)
            Dim numItems As xs_Integer? = If(.Object.Objects Is Nothing, Nothing, .Object.Objects.NumItems)
            
            WriteArray(context, objects, "objects")
            .Add("hasMoreItems", hasMoreItems)
            If numItems.HasValue Then .Add("numItems", numItems.Value)
            If Not String.IsNullOrEmpty(.Object.ChangeLogToken) Then .Add("changeLogToken", .Object.ChangeLogToken)
         End With
      End Sub
   End Class
End Namespace