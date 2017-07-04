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

Namespace CmisObjectModel.Core
   Partial Class cmisRepositoryInfoType
      Public Shared DefaultKeyProperty As New CmisObjectModel.Common.Generic.DynamicProperty(Of cmisRepositoryInfoType, String)(
         Function(item) item._repositoryId,
         Sub(item, value)
            item.RepositoryId = value
         End Sub, "RepositoryId")

#Region "Additional properties for browser-binding"
      ''' <summary>
      ''' Same as property AclCapability; using the BrowserBinding the AclCapability-parameter is called AclCapabilities
      ''' </summary>
      Public Property AclCapabilities As CmisObjectModel.Core.Security.cmisACLCapabilityType
         Get
            Return _aclCapability
         End Get
         Set(value As CmisObjectModel.Core.Security.cmisACLCapabilityType)
            Me.AclCapability = value
         End Set
      End Property

      ''' <summary>
      ''' Same as property PrincipalAnonymous; using the BrowserBinding the PrincipalAnonymous-parameter is called PrincipalIdAnonymous
      ''' </summary>
      Public Property PrincipalIdAnonymous As String
         Get
            Return _principalAnonymous
         End Get
         Set(value As String)
            Me.PrincipalAnonymous = value
         End Set
      End Property

      ''' <summary>
      ''' Same as property PrincipalAnyone; using the BrowserBinding the PrincipalAnyone-parameter is called PrincipalIdAnyone
      ''' </summary>
      Public Property PrincipalIdAnyone As String
         Get
            Return _principalAnyone
         End Get
         Set(value As String)
            Me.PrincipalAnyone = value
         End Set
      End Property

      Private _repositoryUrl As String
      Public Property RepositoryUrl As String
         Get
            Return _repositoryUrl
         End Get
         Set(value As String)
            If _repositoryUrl <> value Then
               Dim oldValue As String = _repositoryUrl
               _repositoryUrl = value
               OnPropertyChanged("RepositoryUrl", value, oldValue)
            End If
         End Set
      End Property 'RepositoryUrl

      Private _rootFolderUrl As String
      Public Property RootFolderUrl As String
         Get
            Return _rootFolderUrl
         End Get
         Set(value As String)
            If _rootFolderUrl <> value Then
               Dim oldValue As String = _rootFolderUrl
               _rootFolderUrl = value
               OnPropertyChanged("RootFolderUrl", value, oldValue)
            End If
         End Set
      End Property 'RootFolderUrl
#End Region
   End Class
End Namespace

Namespace CmisObjectModel.JSON.Core
   Public Class cmisRepositoryInfoTypeConverter
   End Class
End Namespace