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

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Messaging.Requests
   Partial Class RequestBase

#Region "Browser-Binding"
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Class BrowserBindingExtensions
         ''' <summary>
         ''' POST - calls
         ''' </summary>
         Friend Property CmisAction As String
         ''' <summary>
         ''' GET - calls
         ''' </summary>
         Friend Property CmisSelector As String
         ''' <summary>
         ''' If set to true properties will requested in succinct mode
         ''' </summary>
         Public Property Succinct As Boolean = Client.Browser.SuccinctSupport.Current
         ''' <summary>
         ''' Support for getLastRequest within browser binding
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks>see http://docs.oasis-open.org/cmis/CMIS/v1.1/os/CMIS-v1.1-os.html, 5.2.9.2.2 Login and Tokens</remarks>
         Public Property Token As Client.Browser.TokenGenerator = Client.Browser.TokenGenerator.Current
      End Class

      ''' <summary>
      ''' Returns extensions especially designed for browser binding
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property BrowserBinding As BrowserBindingExtensions
         Get
            Static retVal As New BrowserBindingExtensions
            Return retVal
         End Get
      End Property
#End Region

   End Class
End Namespace