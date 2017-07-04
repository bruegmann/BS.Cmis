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
Imports ca = CmisObjectModel.AtomPub
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml

Namespace CmisObjectModel.Data
   Public Class MapperDictionary

      Private _valueMapper As New Dictionary(Of String, Data.Mapper)

      ''' <summary>
      ''' Adds mapping information to the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <param name="mapper"></param>
      ''' <remarks></remarks>
      Public Sub AddMapper(repositoryId As String, mapper As Data.Mapper)
         If mapper IsNot Nothing Then
            If String.IsNullOrEmpty(repositoryId) Then repositoryId = "*"

            SyncLock _valueMapper
               If _valueMapper.ContainsKey(repositoryId) Then
                  _valueMapper(repositoryId) = mapper
               Else
                  _valueMapper.Add(repositoryId, mapper)
               End If
            End SyncLock
         End If
      End Sub

      ''' <summary>
      ''' Removes mapping information from the client
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Sub RemoveMapper(repositoryId As String)
         _valueMapper.Remove(If(String.IsNullOrEmpty(repositoryId), "*", repositoryId))
      End Sub

      ''' <summary>
      ''' Maps the values of properties for the remote system (direction = outgoing) or current system (direction = incoming)
      ''' and returns a delegate to rollback the mapping
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function MapProperties(repositoryId As String, direction As enumMapDirection, ParamArray propertyCollections As Core.Collections.cmisPropertiesType()) As Action
         If propertyCollections Is Nothing Then
            Return Nothing
         Else
            Dim rollbackActions As Action() = (From propertyCollection As Core.Collections.cmisPropertiesType In propertyCollections
                                               Let rollbackAction As Action = MapProperties(repositoryId, direction, propertyCollection)
                                               Where rollbackAction IsNot Nothing
                                               Select rollbackAction).ToArray()
            Select Case rollbackActions.Length
               Case 0
                  Return Nothing
               Case 1
                  Return rollbackActions(0)
               Case Else
                  Return Sub()
                            For Each rollbackAction As Action In rollbackActions
                               rollbackAction.Invoke()
                            Next
                         End Sub
            End Select
         End If
      End Function
      ''' <summary>
      ''' Maps the values of properties for the remote system (direction = outgoing) or current system (direction = incoming)
      ''' and returns a delegate to rollback the mapping
      ''' </summary>
      ''' <param name="properties"></param>
      ''' <param name="direction"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function MapProperties(repositoryId As String, direction As enumMapDirection, properties As Core.Collections.cmisPropertiesType) As Action
         If _valueMapper.ContainsKey(repositoryId) Then
            Return _valueMapper(repositoryId).MapProperties(properties, direction)
         ElseIf _valueMapper.ContainsKey("*") Then
            Return _valueMapper("*").MapProperties(properties, direction)
         Else
            Return Nothing
         End If
      End Function

   End Class
End Namespace