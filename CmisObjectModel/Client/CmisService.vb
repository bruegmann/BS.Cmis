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
Imports ss = System.ServiceModel
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Encapsulates Cmis-Service-Requests to discover the workspaces the client has access to
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisService

      Public Sub New(client As Contracts.ICmisClient)
         _client = client
      End Sub

      Protected _client As Contracts.ICmisClient
      Public ReadOnly Property Client As Contracts.ICmisClient
         Get
            Return _client
         End Get
      End Property

      Protected Overridable Function CreateCmisRepository(repositoryInfo As Core.cmisRepositoryInfoType) As CmisRepository
         Return New CmisRepository(repositoryInfo, _client)
      End Function

      ''' <summary>
      ''' Returns all repositories
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetRepositories() As Dictionary(Of String, Messaging.cmisRepositoryEntryType)
         Dim retVal As New Dictionary(Of String, Messaging.cmisRepositoryEntryType)
         Dim result = _client.GetRepositories(New Messaging.Requests.getRepositories())

         _lastException = result.Exception
         If _lastException Is Nothing AndAlso result.Response IsNot Nothing Then
            For Each entry As Messaging.cmisRepositoryEntryType In result.Response.Repositories
               If entry IsNot Nothing AndAlso Not String.IsNullOrEmpty(entry.RepositoryId) AndAlso Not retVal.ContainsKey(entry.RepositoryId) Then
                  retVal.Add(entry.RepositoryId, entry)
               End If
            Next
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the workspace of specified repository or null
      ''' </summary>
      Public Function GetRepositoryInfo(repositoryId As String) As CmisRepository
         Dim result = _client.GetRepositoryInfo(New Messaging.Requests.getRepositoryInfo() With {.RepositoryId = repositoryId})

         _lastException = result.Exception
         Return If(_lastException Is Nothing AndAlso result.Response IsNot Nothing, CreateCmisRepository(result.Response.RepositoryInfo), Nothing)
      End Function

      Protected _lastException As ss.FaultException
      Public ReadOnly Property LastException As ss.FaultException
         Get
            Return _lastException
         End Get
      End Property

   End Class
End Namespace