using System.Buffers;
using System.IO.Pipelines;

namespace Quick.Protocol;

/// <summary>
/// 包体提供程序
/// </summary>
/// <param name="pipe"></param>
/// <returns></returns>
public delegate ValueTask<int> PackageBodyProvider(Pipe pipe);

/// <summary>
/// 包处理器代理
/// </summary>
/// <param name="channel"></param>
/// <param name="packageType"></param>
/// <param name="bodyBuffer"></param>
/// <returns></returns>
public delegate ValueTask QpPackageHandler(QpChannel channel, byte packageType, ReadOnlySequence<byte> bodyBuffer);

/// <summary>
/// 通知处理器代理
/// </summary>
/// <typeparam name="TNotice"></typeparam>
/// <param name="channel"></param>
/// <param name="notice"></param>
/// <returns></returns>
public delegate ValueTask QpNoticeHandler<TNotice>(QpChannel channel, TNotice notice);
/// <summary>
/// 通知处理器代理
/// </summary>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate ValueTask QpNoticeHandler(QpChannel channel, object notice);

/// <summary>
/// 命令执行器代理
/// </summary>
/// <typeparam name="TCmdRequest"></typeparam>
/// <typeparam name="TCmdResponse"></typeparam>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate ValueTask<TCmdResponse> QpCommandExecuter<TCmdRequest, TCmdResponse>(QpChannel channel, TCmdRequest request);
/// <summary>
/// 命令执行器代理
/// </summary>
/// <param name="channel"></param>
/// <param name="request"></param>
/// <returns></returns>
public delegate ValueTask<object> QpCommandExecuter(QpChannel channel, object request);