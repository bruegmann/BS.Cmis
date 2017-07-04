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
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Collections.Generic
Imports cmr = CmisObjectModel.Messaging.Requests
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
   ''' <summary>
   ''' Simplifies requests to a cmis TypeDefinition
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisType
      Inherits CmisDataModelObject

#Region "Constructors"
      Private Sub New(type As Core.Definitions.Types.cmisTypeDefinitionType,
                      client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         MyBase.New(client, repositoryInfo)
         _type = Type
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Creates the CmisType-Instance
      ''' </summary>
      ''' <remarks></remarks>
      Public Class PreStage
         Public Sub New(client As Contracts.ICmisClient, type As Core.Definitions.Types.cmisTypeDefinitionType)
            _client = client
            _type = type
         End Sub

         Private _client As Contracts.ICmisClient
         Private _type As Core.Definitions.Types.cmisTypeDefinitionType

         Public Shared Operator +(arg1 As PreStage, arg2 As Core.cmisRepositoryInfoType) As CmisType
            Return New CmisType(arg1._type, arg1._client, arg2)
         End Operator
      End Class
#End Region

#Region "Repository"
      ''' <summary>
      ''' Deletes the current type from the repository
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub Delete()
         _lastException = _client.DeleteType(New cmr.deleteType() With {.RepositoryId = _repositoryInfo.RepositoryId, .TypeId = _type.Id}).Exception
      End Sub

      ''' <summary>
      ''' Returns the list of object-types defined for the repository that are children of the current type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetChildren(Optional includePropertyDefinitions As Boolean = False,
                                  Optional maxItems As xs_Integer? = Nothing,
                                  Optional skipCount As xs_Integer? = Nothing) As Generic.ItemList(Of CmisType)
         Return MyBase.GetTypeChildren(_type.Id, includePropertyDefinitions, maxItems, skipCount)
      End Function

      ''' <summary>
      ''' Returns the set of the descendant object-types defined for the Repository under the current type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(Optional depth As xs_Integer? = Nothing,
                                     Optional includePropertyDefinitions As Boolean = False) As Generic.ItemContainer(Of CmisType)()
         Return GetTypeDescendants(_type.Id, depth, includePropertyDefinitions)
      End Function

      ''' <summary>
      ''' Updates the current type definition and returns True in case of success
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Update(type As Core.Definitions.Types.cmisTypeDefinitionType) As Boolean
         With _client.UpdateType(New cmr.updateType() With {.RepositoryId = _repositoryInfo.RepositoryId, .Type = type})
            _lastException = .Exception
            If _lastException Is Nothing Then
               type = .Response.Type
               If type IsNot Nothing Then
                  _type = type
                  Return True
               End If
            End If

            Return False
         End With
      End Function
#End Region

#Region "Browser Binding support"
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never),
       Obsolete("No succinct representation defined for CmisType", True)>
      Public Overrides Sub BeginSuccinct(succinct As Boolean)
      End Sub
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never),
       Obsolete("No succinct representation defined for CmisType", True)>
      Public Overrides ReadOnly Property CurrentSuccinct As Boolean
         Get
            Return False
         End Get
      End Property
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never),
       Obsolete("No succinct representation defined for CmisType", True)>
      Public Overrides Function EndSuccinct() As Boolean
         Return False
      End Function
#End Region

      ''' <summary>
      ''' Access to PropertyDefinitions via index or Id
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PropertyDefinitionsAsReadOnly As ccg.ArrayMapper(Of ccdt.cmisTypeDefinitionType, ccdp.cmisPropertyDefinitionType)
         Get
            Return _type.PropertyDefinitionsAsReadOnly
         End Get
      End Property

      Protected _type As Core.Definitions.Types.cmisTypeDefinitionType
      Public ReadOnly Property Type As Core.Definitions.Types.cmisTypeDefinitionType
         Get
            Return _type
         End Get
      End Property

   End Class
End Namespace