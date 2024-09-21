using Zenject;

public class ClockInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ClockHandRedactor>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ClockHandController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ClockHand>().FromComponentInHierarchy().AsTransient();
        Container.Bind<TimeManager>().FromComponentInHierarchy().AsSingle();
    }
}