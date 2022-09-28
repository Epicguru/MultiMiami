using System.Reflection;

namespace MM.Multiplayer.Remote
{
    public partial class Prefixes_NETGEN
    {
        private static Dictionary<Type, MethodInfo> delegates = new Dictionary<Type, MethodInfo>();

        [ClientRPC]
        public void Test()
        {
        }

        [ClientRPC]
        private void Test2()
        {
        }

        [ClientRPC]
        internal void Test3()
        {
        }

        [ClientRPC]
        void Test4()
        {
        }
    }
}
