using OneOf;



namespace SwitchApps.Library.Registry.Model
{


    public class RegistryItemValue : OneOfBase<int, string>
    {
        public RegistryItemValue(OneOf<int, string> _) : base(_) { }

        public static implicit operator RegistryItemValue(int _) => new RegistryItemValue(_);
        public static explicit operator int(RegistryItemValue _) => _.AsT0;

        public static implicit operator RegistryItemValue(string _) => new RegistryItemValue(_);
        public static explicit operator string(RegistryItemValue _) => _.AsT1;
    }
}