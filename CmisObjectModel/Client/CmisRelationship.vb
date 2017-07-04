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
Imports CmisObjectModel.Constants
Imports ccg = CmisObjectModel.Common.Generic
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   Public Class CmisRelationship
      Inherits CmisObject

#Region "Constructors"
      Public Sub New(cmisObject As Core.cmisObjectType,
                     client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         MyBase.New(cmisObject, client, repositoryInfo)
      End Sub
#End Region

#Region "Predefined properties"
      Public Overridable Property SourceId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.SourceId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.SourceId = value
         End Set
      End Property

      Public Overridable Property TargetId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.TargetId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.TargetId = value
         End Set
      End Property
#End Region

   End Class
End Namespace