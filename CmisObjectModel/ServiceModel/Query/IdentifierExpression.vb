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
Imports ccg = CmisObjectModel.Common.Generic
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Contains the name of a database object (table or column)
   ''' </summary>
   ''' <remarks></remarks>
   Public Class IdentifierExpression
      Inherits Expression

#Region "Constructors"
      Public Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer)
         MyBase.New(match, groupName, rank, index)

         Dim groupAny = match.Groups("Any")
         _anyIsPresent = groupAny IsNot Nothing AndAlso groupAny.Success
         _identifier = match.Groups(groupName).Value
      End Sub
#End Region

      Protected _anyIsPresent As Boolean
      Public ReadOnly Property AnyIsPresent As Boolean
         Get
            Return _anyIsPresent
         End Get
      End Property

      Protected _identifier As String
      Public ReadOnly Property Identifier As String
         Get
            Return _identifier
         End Get
      End Property

   End Class
End Namespace