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
Imports ss = System.ServiceModel
Imports ssd = System.ServiceModel.Description
Imports sss = System.ServiceModel.Security

Namespace CmisObjectModel.ServiceModel.Browser
   ''' <summary>
   ''' Opens and closes servicehosts for baseAdresses
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ServiceManager
      Inherits Base.ServiceManager

      Protected Overrides Function GetImplementedContractType() As System.Type
         Return GetType(Contracts.IBrowserBinding)
      End Function

      Protected Overrides Function GetServiceType() As System.Type
         Return GetType(CmisService)
      End Function

      Protected Overrides Function SupportsClientCredentialType() As Boolean
         Return False
      End Function

   End Class
End Namespace