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
   ''' Converter for RowCollection-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class RowCollectionConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Extensions.Data.RowCollection)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Extensions.Data.RowCollection))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Extensions.Data.RowCollection))
         MyBase.New(objectResolver)
      End Sub
#End Region

#Region "Helper-classes"
      Private NotInheritable Class RowCollectionWriter
         Inherits CmisObjectModel.Extensions.Data.RowCollection

         Private Sub New()
            MyBase.New()
         End Sub

         Public Shared Sub Write(instance As CmisObjectModel.Extensions.Data.RowCollection,
                                 rowIndexPropertyDefinitionId As String,
                                 rowTypeId As String, tableName As String,
                                 rows As CmisObjectModel.Extensions.Data.Row())
            CmisObjectModel.Extensions.Data.RowCollection.SilentInitialization(instance, rowIndexPropertyDefinitionId, rowTypeId, tableName, rows)
         End Sub
      End Class
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim rows As CmisObjectModel.Extensions.Data.Row() = Nothing

            rows = ReadArray(context, "rows", rows)
            RowCollectionWriter.Write(.Object,
                                      Read(.Dictionary, "rowIndexPropertyDefinitionId", .Object.RowIndexPropertyDefinitionId),
                                      Read(.Dictionary, "rowTypeId", .Object.RowTypeId),
                                      Read(.Dictionary, "tableName", .Object.TableName), rows)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            .Add("extensionTypeName", .Object.ExtensionTypeName)
            WriteArray(context, .Object.Rows, "rows")
            .Add("rowIndexPropertyDefinitionId", .Object.RowIndexPropertyDefinitionId)
            .Add("rowTypeId", .Object.RowTypeId)
            .Add("tableName", .Object.TableName)
         End With
      End Sub
   End Class
End Namespace