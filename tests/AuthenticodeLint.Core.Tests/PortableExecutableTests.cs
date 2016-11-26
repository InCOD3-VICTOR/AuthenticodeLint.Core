using Xunit;
using AuthenticodeLint.Core.PE;
using System.Threading.Tasks;
using System.IO;

namespace AuthenticodeLint.Core.Tests
{
    public class PortableExecutableTests
    {
        [Fact]
        public async Task ShouldReadSimpleAttributesOfPE()
        {
            using (var pe = new PortableExecutable("files/authlint.exe"))
            {
                var header = await pe.GetDosHeader();
                Assert.NotEqual(0, header.ExeFileHeaderAddress);

                var peHeader = await pe.GetPeHeader(header);
                Assert.Equal(MachineArchitecture.x86, peHeader.Architecture);
                Assert.Equal(16, peHeader.DataDirectories.Count);

                var securityHeader = peHeader.DataDirectories[ImageDataDirectoryEntry.IMAGE_DIRECTORY_ENTRY_SECURITY];
                Assert.NotEqual(0, securityHeader.Size);
                Assert.NotEqual(0, securityHeader.VirtualAddress);
            }
        }

        [Fact]
        public async Task ShouldReadSecuritySection()
        {
            using (var pe = new PortableExecutable("files/authlint.exe"))
            {
                var header = await pe.GetDosHeader();
                var peHeader = await pe.GetPeHeader(header);
                var securityHeader = peHeader.DataDirectories[ImageDataDirectoryEntry.IMAGE_DIRECTORY_ENTRY_SECURITY];
                using (var file = new FileStream("files/authlint.exe", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    SecuritySection section;
                    var result = SecuritySection.ReadSection(file, securityHeader, out section);
                    Assert.True(result);
                    Assert.Equal(0x30, section.Data[0]);
                }
            }
        }
    }
}