using System;
using System.Linq;
using System.Reflection;
using Zenject;

public class SaveLoadInstaller : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle().NonLazy();

        InstalRepos();
    }

    private void InstalRepos() {
        var assembly = Assembly.GetExecutingAssembly();
        var dataTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<DataSourceAttribute>() != null);

        foreach (var dataType in dataTypes) {
            var attribute = dataType.GetCustomAttribute<DataSourceAttribute>();
            var repositoryInterface = typeof(IDataRepository<>).MakeGenericType(dataType);
            var repositoryImpl = GetRepoType(attribute.SourceType).MakeGenericType(dataType);

            Container.Bind(repositoryInterface)
                .To(repositoryImpl)
                .AsSingle()
                .WithArguments(attribute.Key);
        }
    }

    private Type GetRepoType(DataSourceType source) {
        return source switch {
            DataSourceType.PlayerPrefs => typeof(PlayerPrefsRepository<>),
            DataSourceType.Resources => typeof(ResourcesRepository<>),
            DataSourceType.FileSystem => typeof(JsonDataRepository<>),
            _ => throw new NotSupportedException()
        };
    }
}