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

Namespace CmisObjectModel.Core
   Partial Public Class cmisObjectIdAndChangeTokenType

      ''' <summary>
      ''' Converts AtomEntry of object
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Widening Operator CType(value As AtomPub.AtomEntry) As cmisObjectIdAndChangeTokenType
         'see 3.8.6.1 HTTP POST:
         'The property cmis:objectId MUST be set.
         'The value MUST be the original object id even if the repository created a new version and therefore generated a new object id.
         'New object ids are not exposed by AtomPub binding. 
         'The property cmis:changeToken MUST be set if the repository supports change tokens
         Return If(value Is Nothing, Nothing, New cmisObjectIdAndChangeTokenType() With {._id = value.ObjectId, ._changeToken = value.ChangeToken})
      End Operator

   End Class
End Namespace