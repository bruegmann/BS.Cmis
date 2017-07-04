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
   ''' Converter for Row-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class RowConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Extensions.Data.Row)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Extensions.Data.Row))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Extensions.Data.Row))
         MyBase.New(objectResolver)
      End Sub
#End Region

#Region "Helper-classes"
      Private NotInheritable Class RowWriter
         Inherits CmisObjectModel.Extensions.Data.Row

         Private Sub New()
            MyBase.New()
         End Sub

         Public Shared Sub Write(instance As CmisObjectModel.Extensions.Data.Row,
                                 properties As CmisObjectModel.Core.Collections.cmisPropertiesType,
                                 originalProperties As CmisObjectModel.Core.Collections.cmisPropertiesType,
                                 rowState As DataRowState)
            CmisObjectModel.Extensions.Data.Row.SilentInitialization(instance, properties, originalProperties, rowState)
         End Sub
      End Class
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim originalProperties As CmisObjectModel.Core.Collections.cmisPropertiesType =
               Read(Of CmisObjectModel.Core.Collections.cmisPropertiesType)(context, "originalProperties", Nothing)
            Dim properties As CmisObjectModel.Core.Collections.cmisPropertiesType =
               Read(Of CmisObjectModel.Core.Collections.cmisPropertiesType)(context, "properties", Nothing)
            Dim rowState As DataRowState = ReadEnum(.Dictionary, "rowState", DataRowState.Detached)

            'serialization suppressed originalProperties for rows in Unchanged-state
            If rowState = DataRowState.Unchanged AndAlso properties IsNot Nothing Then
               originalProperties = CType(properties.Copy(), CmisObjectModel.Core.Collections.cmisPropertiesType)
            End If
            RowWriter.Write(context.Object, properties, originalProperties, rowState)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            'serialization of originalProperties only for states which support differences between
            'properties and originalProperties
            If .Object.RowState = DataRowState.Deleted OrElse .Object.RowState = DataRowState.Modified Then
               Write(context, .Object.GetOriginalProperties(), "originalProperties")
            End If
            Write(context, .Object.Properties, "properties")
            .Add("rowState", .Object.RowState.GetName())
         End With
      End Sub

   End Class
End Namespace