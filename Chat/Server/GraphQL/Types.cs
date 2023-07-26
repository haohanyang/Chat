using Chat.Server.Data.Entity;
using Chat.Server.Services.Interface;
namespace Chat.Server.GraphQL;

public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor.Field(e => e.Id);
        descriptor.Field(e => e.UserName);
        descriptor.Field(e => e.FirstName);
        descriptor.Field(e => e.LastName);
        descriptor.Field(e => e.Avatar);
        descriptor.Field(e => e.Bio);
        descriptor.Field(e => e.DirectChats1);
        descriptor.Field(e => e.DirectChats2);
        descriptor.Field(e => e.SpaceMemberships);
    }
}

public class SpaceMessageAttachmentExtension : ObjectTypeExtension<SpaceMessageAttachment>
{
    protected override void Configure(IObjectTypeDescriptor<SpaceMessageAttachment> descriptor)
    {
        descriptor
            .Field("uri")
            .Type<StringType>()
            .Resolve(context =>
            {
                var fileService = context.Service<IFileService>();
                var attachment = context.Parent<DirectMessageAttachment>();
                return fileService.GetBlobCDNUrl(attachment.BlobName);
            });
    }
}

public class DirectMessageAttachmentExtension : ObjectTypeExtension<DirectMessageAttachment>
{
    protected override void Configure(IObjectTypeDescriptor<DirectMessageAttachment> descriptor)
    {
        descriptor
            .Field("uri")
            .Type<StringType>()
            .Resolve(context =>
            {
                var fileService = context.Service<IFileService>();
                var attachment = context.Parent<DirectMessageAttachment>();
                return fileService.GetBlobCDNUrl(attachment.BlobName);
            });
    }
}