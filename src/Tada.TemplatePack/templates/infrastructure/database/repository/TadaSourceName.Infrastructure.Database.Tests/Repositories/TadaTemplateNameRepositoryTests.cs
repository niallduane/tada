using System;
using Microsoft.EntityFrameworkCore;
using TadaSourceName.Domain.Core;
using TadaSourceName.Infrastructure.Database.Entities;
using TadaSourceName.Infrastructure.Database.Repositories.TadaTemplateNames;
using TadaSourceName.Infrastructure.Database.Tests.Factories;

namespace TadaSourceName.Infrastructure.Database.Tests;

public class TadaTemplateNameRepositoryTests
{
    private readonly InMemoryDatabaseContext _dbContext = new();
    private readonly TadaTemplateNameRepository? _repository;
    private readonly TadaTemplateNameFactory _tadatemplatenameFactory;

    public TadaTemplateNameRepositoryTests()
    {
        _repository = new TadaTemplateNameRepository(_dbContext);
        _tadatemplatenameFactory = new TadaTemplateNameFactory();
    }

    [Fact]
    public async Task GetTadaTemplateName_Success()
    {
        var expected = await _dbContext.TadaTemplateNames.FirstAsync();

        var result = await _repository!.GetTadaTemplateName(expected.TadaTemplateNameId);

        Assert.NotNull(result);
        Assert.Equal(expected.TadaTemplateNameId, result.TadaTemplateNameId);
    }

    [Fact]
    public async Task ListTadaTemplateNames_Success()
    {
        var expected = await _dbContext.TadaTemplateNames.ToListAsync();
        var request = new BaseListRequest
        {

        };
        var result = await _repository!.ListTadaTemplateNames(request);

        Assert.NotNull(result);
        //todo: add more test asserts
    }

    [Fact]
    public async Task Create_Success()
    {
        var request = _tadatemplatenameFactory.Generate();
        var result = await _repository!.Create(request);

        Assert.NotNull(result);
        //todo: add more test asserts
    }

    [Fact]
    public async Task Update_Success()
    {
        var item = await _dbContext.TadaTemplateNames.FirstAsync();
        var result = await _repository!.Update(item.TadaTemplateNameId, new Dictionary<string, object>()
        {
            //Todo: Add properties to update
        });

        Assert.NotNull(result);
        Assert.Equal(item.TadaTemplateNameId, result.TadaTemplateNameId);

        //Todo: Add Asserts to check update
    }

    [Fact]
    public async Task Delete_Success()
    {
        var expected = await _dbContext.TadaTemplateNames.FirstAsync();

        await _repository!.Delete(expected.TadaTemplateNameId);

        var result = await _dbContext.TadaTemplateNames.FirstOrDefaultAsync(item => item.TadaTemplateNameId == expected.TadaTemplateNameId);

        Assert.Null(result);
    }
}
