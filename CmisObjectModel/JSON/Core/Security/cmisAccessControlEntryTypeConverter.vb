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
Imports ccs = CmisObjectModel.Core.Security
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Security
   <Attributes.JavaScriptConverter(GetType(JSON.Core.Security.cmisAccessControlEntryTypeConverter))>
   Partial Class cmisAccessControlEntryType
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core.Security
   Public Class cmisAccessControlEntryTypeConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of ccs.cmisAccessControlEntryType)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New Serialization.Generic.DefaultObjectResolver(Of ccs.cmisAccessControlEntryType))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of ccs.cmisAccessControlEntryType))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            .Object.Principal = Read(context, "principal", .Object.Principal)
            .Object.Permissions = ReadArray(.Dictionary, "permissions", .Object.Permissions)
            .Object.IsDirect = Read(.Dictionary, "isDirect", .Object.IsDirect)
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            Write(context, .Object.Principal, "principal")
            WriteArray(context, .Object.Permissions, "permissions")
            .Add("isDirect", .Object.IsDirect)
         End With
      End Sub
   End Class
End Namespace