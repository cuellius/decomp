using System.Globalization;
using System.IO;

namespace Decomp.Core.WSE2
{
    public static class PhysicsMaterials
    {
        public static void Decompile()
        {
            var fMaterials = new Win32BinaryFileReader(Path.Combine(Common.InputPath, "core_physics_materials.brf"));
            var fSource = new Win32FileWriter(Path.Combine(Common.OutputPath, "resource_physics_materials.py"));
            fSource.WriteLine(Header.Standard);
            fSource.WriteLine(Header.PhysicsMaterials);

            // 6 + 4 = 10 for "rfver "
            // 4 for 1 (__int32)
            // 16 + 4 = 20 for "physics_material"
            fMaterials.SkipBytes(34);
            var iMaterials = fMaterials.ReadInt32();

            for (int m = 0; m < iMaterials; m++)
            {
                var name = fMaterials.ReadAsciiString();
                var intValue = fMaterials.ReadInt32();
                var floatValue = fMaterials.ReadFloat();
                fSource.WriteLine("  (\"{0}\", {1}, {2}),", name, intValue, 
                    floatValue.ToString(CultureInfo.GetCultureInfo("en-US")));
            }

            fSource.Write("]");
            fSource.Close();
            fMaterials.Close();
        }
    }
}
