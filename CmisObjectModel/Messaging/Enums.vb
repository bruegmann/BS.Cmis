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
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Messaging
   Public Enum enumDeleteTreeResult As Integer
      OK = System.Net.HttpStatusCode.OK
      Accepted = System.Net.HttpStatusCode.Accepted
      NoContent = System.Net.HttpStatusCode.NoContent
      Unauthorized = System.Net.HttpStatusCode.Unauthorized
      Forbidden = System.Net.HttpStatusCode.Forbidden
      InternalServerError = System.Net.HttpStatusCode.InternalServerError
   End Enum

   Public Enum enumGetContentStreamResult As Integer
      Content = System.Net.HttpStatusCode.OK
      PartialContent = System.Net.HttpStatusCode.PartialContent

      NotSet = System.Net.HttpStatusCode.InternalServerError
   End Enum

   Public Enum enumSetContentStreamResult As Integer
      HasContent = System.Net.HttpStatusCode.OK
      NoContent = System.Net.HttpStatusCode.NoContent
      Created = System.Net.HttpStatusCode.Created

      NotSet = System.Net.HttpStatusCode.InternalServerError
   End Enum
End Namespace