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
Imports st = System.Text
Imports str = System.Text.RegularExpressions

Namespace CmisObjectModel.ServiceModel.Query
   ''' <summary>
   ''' Base-class for sql-expressions (orderBy-expression, select-expression, from-expression and where-expression)
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class SqlPartExpression
      Inherits CompositeExpression

#Region "Constructors"
      Protected Sub New(match As str.Match, groupName As String, rank As Integer, index As Integer,
                        childrenSeparator As String, childBlockSeparator As String)
         MyBase.New(match, groupName, rank, index, childrenSeparator, childBlockSeparator)
      End Sub
#End Region

      ''' <summary>
      ''' Returns the SqlExpression of the sql-part without the introductory keyword
      ''' </summary>
      Public Function GetSqlExpression() As String
         Return String.Join(If(_childrenSeparator, ""),
                            From child As Expression In _children
                            Let childExpression As String = child.Value
                            Select childExpression)
      End Function

   End Class
End Namespace