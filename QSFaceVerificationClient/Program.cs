using Google.Protobuf;
using Grpc.Net.Client;

using System;
using System.IO;
using System.Threading.Tasks;

namespace QS.Face.Verification.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);

            using var channel = GrpcChannel.ForAddress(args[0]);
            var client = new FaceVerification.FaceVerificationClient(channel);

            using (var fs1 = File.OpenRead(args[1]))
            using (var fs2 = File.OpenRead(args[2]))
            {

                try
                {
                    var result = await client.VerifyAsync(new VerifyRequest
                    {
                        ImageData1 = await ByteString.FromStreamAsync(fs1),
                        ImageData2 = await ByteString.FromStreamAsync(fs2),
                    });

                    Console.WriteLine("Verification result: {0}", result.Status);
                    switch (result.Status)
                    {
                        case VerifyReply.Types.ReplyStatus.None:
                            break;

                        case VerifyReply.Types.ReplyStatus.Ok:
                            Console.WriteLine("  Score: {0}, Confidence: {1}", result.Result.Score, result.Result.Confidence);
                            break;

                        case VerifyReply.Types.ReplyStatus.InputDataError:
                        case VerifyReply.Types.ReplyStatus.InternalError:

                            Console.WriteLine("  Message: {0}, {2}  Trace: {1}", result.Error.Message, result.Error.Trace, Environment.NewLine);
                            break;

                        default:
                            break;
                    }

                }catch (Exception exc)
                {
                    Console.Error.WriteLine(exc);
                }
            }
        }
    }
}
