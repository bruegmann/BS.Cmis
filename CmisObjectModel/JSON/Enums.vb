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
Imports sn = System.Net
Imports sw = System.Web

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON
   <HideModuleName()>
   Public Module Enums
      Public Enum enumCollectionAction As Integer
         add
         remove
      End Enum

      Public Enum enumJSONFile As Integer
         cmisJS
         embeddedFrameHtm
         emptyPageHtm
         loginPageHtm
         loginRefPageHtm
      End Enum

      <Flags()>
      Public Enum enumJSONPredefinedParameter As Integer
         callback = minValue
         cmisaction = callback << 1
         cmisselector = cmisaction << 1
         succinct = cmisselector << 1
         suppressResponseCodes = succinct << 1
         token = suppressResponseCodes << 1

         minValue = &H10000
         maxValue = token
      End Enum

      <Flags()>
      Public Enum enumRequestParameterSource As Integer
         multipart = 1
         queryString = 2
      End Enum

      Public Enum enumValueType As Integer
         changeToken 'only applies to bulkUpdateProperties
         objectId 'only applies to bulkUpdateProperties
         policy
      End Enum
   End Module
End Namespace