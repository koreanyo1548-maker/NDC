// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("/MmDN0movfn4F8EsNerlRDk0i/0xsryzgzGyubExsrKzLwt+u2QcuZ3DVdTp5b5IbdqB1hQu/MAdgw9QL+6f8MtPxl20Da3MFk5lcjUwvJDvOgg0OX2b5J/YCbnXEl3XpfUJZH/+hhxK4v9FInq6i7odkSe5xT96gzGykYO+tbqZNfs1RL6ysrK2s7C3LmcHIIjgPMIc2jtRi5m1J2Ic4Npcd64iV9FqtZ2mJKV6RIrEzqW2kYB6SE1L8HTS9zdaWD9zZE/L217dOdXHZACIVtzcLJ+GiESvjx4+XRgVGZS3i7zRaC4ci2EahvfRYP5aWAiGQAnFGUSz9FQrTTGkgGrRSqEWZYKck3Vq3mYK/oIfCcJkB4ntlQjbZevBEHWhpLGwsrOy");
        private static int[] order = new int[] { 9,1,13,12,7,6,9,10,11,10,10,13,12,13,14 };
        private static int key = 179;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
