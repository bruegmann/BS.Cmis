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
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Common.Generic
   ''' <summary>
   ''' Generic UriBuilder
   ''' </summary>
   ''' <typeparam name="TEnum"></typeparam>
   ''' <remarks></remarks>
   Public Class LinkUriBuilder(Of TEnum As Structure)

      Private _baseServiceUri As String
      Protected _flags As Integer = 0
      Protected _searchAndReplace As List(Of String)
      Private Shared _values As Dictionary(Of Integer, Tuple(Of String, System.Enum)) = ServiceURIs.GetValues(GetType(TEnum))

#Region "Constructors"
      Protected Sub New()
      End Sub
      Public Sub New(baseServiceUri As String)
         _baseServiceUri = baseServiceUri
         _searchAndReplace = New List(Of String)
      End Sub
      Public Sub New(baseServiceUri As String, repositoryId As String)
         _baseServiceUri = baseServiceUri
         _searchAndReplace = New List(Of String) From {"repositoryId", repositoryId}
      End Sub
#End Region

#Region "Add overloads"
      Public Sub Add(flag As TEnum, value As Boolean)
         Add(flag, If(value, "true", "false"))
      End Sub
      Public Sub Add(flag As TEnum, value As Boolean?)
         If value.HasValue Then Add(flag, value.Value) Else Add(flag, CStr(Nothing))
      End Sub
      Public Sub Add(Of TValue As Structure)(flag As TEnum, value As TValue)
         Add(flag, CType(CObj(value), System.Enum).GetName())
      End Sub
      Public Sub Add(Of TValue As Structure)(flag As TEnum, value As TValue?)
         If value.HasValue Then Add(flag, value.Value) Else Add(flag, CStr(Nothing))
      End Sub
      Public Sub Add(flag As TEnum, value As Integer)
         Add(flag, CStr(value))
      End Sub
      Public Sub Add(flag As TEnum, value As Integer?)
         If value.HasValue Then Add(flag, value.Value) Else Add(flag, CStr(Nothing))
      End Sub
      Public Sub Add(flag As TEnum, value As Long)
         Add(flag, CStr(value))
      End Sub
      Public Sub Add(flag As TEnum, value As Long?)
         If value.HasValue Then Add(flag, value.Value) Else Add(flag, CStr(Nothing))
      End Sub
      Public Sub Add(flag As TEnum, value As String)
         If Not String.IsNullOrEmpty(value) Then
            Dim currentFlag As Integer = CInt(CObj(flag))

            If _values.ContainsKey(currentFlag) Then
               _flags = _flags Or currentFlag
               _searchAndReplace.Add(GetName(_values(currentFlag).Item2))
               _searchAndReplace.Add(value)
            End If
         End If
      End Sub
#End Region

      Public Overridable Function ToUri() As Uri
         Return New Uri(ServiceURIs.GetServiceUri(_baseServiceUri, CType(CObj(_flags), TEnum)).ReplaceUri(_searchAndReplace.ToArray()))
      End Function

   End Class
End Namespace