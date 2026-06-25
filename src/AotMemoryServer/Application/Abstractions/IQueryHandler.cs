namespace AotMemoryServer.Application.Abstractions;

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> Handle(TQuery query);
}
