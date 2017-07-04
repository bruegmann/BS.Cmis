Public Class Metadata

   ''' <summary>
   ''' Value of Property "cmis:description" for the latest Version
   ''' </summary>
   ''' <remarks></remarks>
   Public Description As String

   ''' <summary>
   ''' Value of the Property "cmis:description" for the Private Working Copy (PWC)
   ''' </summary>
   ''' <remarks></remarks>
   Public DescriptionPwc As String

   ''' <summary>
   ''' Value of Property "patorg:akte" for the latest Version
   ''' </summary>
   ''' <remarks></remarks>
   Public Akte() As String

   ''' <summary>
   ''' Value of the Property "patorg:akte" for the Private Working Copy (PWC)
   ''' </summary>
   ''' <remarks></remarks>
   Public AktePwc() As String

   Public MajorOfLatestVersion As Integer
   Public MinorOfLatestVersion As Integer

   Public MimeType As String

   Public CreatedBy As String
   Public CreationDate As DateTime
   Public LastModifiedBy As String
   Public LastModificationDate As DateTime

   Public ForeignChangeToken As String

   Public VersionSeriesCheckedOutBy As String

   Public ReadOnly Property IsVersionSeriesCheckedOut As Boolean
      Get
         Return Not String.IsNullOrEmpty(VersionSeriesCheckedOutBy)
      End Get
   End Property

   Public ReadOnly Property LabelOfLatestVersion As String
      Get
         Return MajorOfLatestVersion & "." & MinorOfLatestVersion
      End Get
   End Property

#Region "Comments"

   Public CheckinComments As CheckinComment()

   Public Class CheckinComment
      Public VersionLabel As String
      Public Comment As String
   End Class

   Public Sub AddComment(text As String)
      Dim list As New List(Of Metadata.CheckinComment)
      If CheckinComments IsNot Nothing Then
         list.AddRange(CheckinComments)
      End If
      list.Add(New CheckinComment() With {.VersionLabel = LabelOfLatestVersion, .Comment = text})
      CheckinComments = list.ToArray()
   End Sub

   Public Function GetComment(Optional versionLabel As String = Nothing) As String
      If String.IsNullOrEmpty(versionLabel) Then versionLabel = LabelOfLatestVersion
      If CheckinComments IsNot Nothing Then
         For Each comment As CheckinComment In CheckinComments
            If comment.VersionLabel.Equals(versionLabel) Then
               Return comment.Comment
            End If
         Next
      End If
      Return Nothing
   End Function

#End Region

#Region "Xml"

   Public Shared Function FromXml(xml As String) As Metadata
      Dim serializer As New Xml.Serialization.XmlSerializer(GetType(Metadata))
      Dim meta As Metadata = CType(serializer.Deserialize(New IO.StringReader(xml)), Metadata)
      Return meta
   End Function

   Public Function ToXml()
      Dim serializer As New Xml.Serialization.XmlSerializer(GetType(Metadata))
      Dim writer As New IO.StringWriter()
      serializer.Serialize(writer, Me)
      Dim xml = writer.ToString()
      Return xml
   End Function

#End Region

End Class
