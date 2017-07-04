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

Namespace CmisObjectModel.Messaging
   Partial Public Class cmisRepositoryEntryType

      Protected _repository As Core.cmisRepositoryInfoType
      Public Property Repository As Core.cmisRepositoryInfoType
         Get
            If _repository Is Nothing Then _repository = New Core.cmisRepositoryInfoType With {.RepositoryId = _repositoryId, .RepositoryName = _repositoryName}
            Return _repository
         End Get
         Set(value As Core.cmisRepositoryInfoType)
            If _repository IsNot value Then
               Dim oldValue As Core.cmisRepositoryInfoType = _repository
               _repository = value
               OnPropertyChanged("Repository", value, oldValue)
               If value Is Nothing Then
                  Me.RepositoryId = Nothing
                  Me.RepositoryName = Nothing
               Else
                  Me.RepositoryId = value.RepositoryId
                  Me.RepositoryName = value.RepositoryName
               End If
            End If
         End Set
      End Property

      Public Shared Widening Operator CType(value As Core.cmisRepositoryInfoType) As cmisRepositoryEntryType
         Return If(value Is Nothing, Nothing, New cmisRepositoryEntryType With {._repository = value, ._repositoryId = value.RepositoryId, ._repositoryName = value.RepositoryName})
      End Operator

   End Class
End Namespace