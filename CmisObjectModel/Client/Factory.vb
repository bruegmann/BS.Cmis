'*******************************************************************************************
'* Copyright Brügmann Software GmbH, Papenburg
'* Author: BSW_COL
'* Contact: codeplex<at>patorg.de
'* 
'* VB.CMIS is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of VB.CMIS.
'* 
'* VB.CMIS is free software: you can redistribute it and/or modify
'* it under the terms of the GNU Lesser General Public License as published by
'* the Free Software Foundation, either version 3 of the License, or
'* (at your option) any later version.
'* 
'* VB.CMIS is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'* GNU Lesser General Public License for more details.
'* 
'* You should have received a copy of the GNU Lesser General Public License
'* along with VB.CMIS. If not, see <http://www.gnu.org/licenses/>.
'*******************************************************************************************
Imports CmisObjectModel.Common

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Methods to create CMIS-Clients for all Bindings
   ''' </summary>
   ''' <remarks></remarks>
   Public Module Factory

      ''' <summary>
      ''' Creates an instance of CMIS-CLient
      ''' </summary>
      ''' <param name="clientType"></param>
      ''' <param name="serviceDocUri"></param>
      ''' <param name="vendor"></param>
      ''' <param name="authentication"></param>
      ''' <param name="connectTimeout"></param>
      ''' <param name="readWriteTimeout"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateClient(clientType As enumClientType,
                                   serviceDocUri As Uri, vendor As enumVendor,
                                   authentication As AuthenticationProvider,
                                   Optional connectTimeout As Integer? = Nothing,
                                   Optional readWriteTimeout As Integer? = Nothing) As Contracts.ICmisClient

         Dim retVal As Contracts.ICmisClient = Nothing

         Select Case clientType
            Case enumClientType.AtomPub
               retVal = New AtomPub.CmisClient(serviceDocUri, vendor, authentication, connectTimeout, readWriteTimeout)
            Case enumClientType.BrowserBinding
               retVal = New Browser.CmisClient(serviceDocUri, vendor, authentication, connectTimeout, readWriteTimeout)
               ' Case enumClientType.WebService

         End Select

         Return retVal

      End Function

   End Module

End Namespace
