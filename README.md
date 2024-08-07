# GRFEditor
An editor for the GRF/GPF/Thor file formats from Ragnarok Online.

### Using the GRF library
#### Setting up the ErrorHandler
With a new project, the first thing you'll want to do is look into the ErrorHandler.
By default, the ErrorHandler is set to use ErrorManager.DefaultHandler which uses the Console window and may not be very helpful.
GRF Editor uses GrfToWpfBridge.Application.DefaultErrorHandler which uses a WPF based interface.
If you want to handle the exceptions yourself, you can simply rethrow them with the RethrowErrorHandler shown below (or make your own).
```
public class RethrowErrorHandler : IErrorHandler {
	public void Handle(Exception exception, ErrorLevel errorLevel) {
		throw exception;
	}

	public bool YesNoRequest(string message, string caption) {
		if (MessageBox.Show("The application requires your attention.\n\n" + message, caption, MessageBoxButtons.YesNo) == DialogResult.Yes)
			return true;

		return false;
	}
}

public class GrfTest {
	public GrfTest() {
		ErrorHandler.SetErrorHandler(new DefaultHandler());
		ErrorHandler.SetErrorHandler(new RethrowErrorHandler());
		ErrorHandler.SetErrorHandler(new DefaultErrorHandler());
	}
}
```
#### Using GrfHolder
The GrfHolder is the main class for handling GRF files. All operations on the GRF must be applied by using the Commands object. You can find all the available methods in GRF.ContainerFormat.Commands.CommandsHolder.cs.
```
// Add and remove files
GrfHolder grf = new GrfHolder(@"C:\data.grf");

grf.Commands.AddFile(@"data\texture\grid.tga", @"C:\test\custom_grid.tga");
grf.Commands.RemoveFile(@"data\texture\loading00.jpg");

if (grf.Commands.CanUndo)
	grf.Commands.Undo();

if (grf.Commands.CanRedo)
	grf.Commands.Redo();

grf.QuickSave();
// Reload is only necessary if you plan on using the GRF again after saving it.
grf.Reload();
```

The entries from the GRF are stored in the FileTable object and can be extracted as follows:
```
GrfHolder grf = new GrfHolder(@"C:\data.grf");

var entry = grf.FileTable.TryGet(@"data\texture\grid.tga");

if (entry == null)
	throw new Exception("Entry not found.");

// You can also get the entry directly as follow
entry = grf.FileTable[@"data\texture\grid.tga"];

var data = entry.GetDecompressedData();

File.WriteAllBytes(@"C:\test\custom_grid.tga", data);
```

You can also iterate through a specific folder in the GRF.
```
GrfHolder grf = new GrfHolder(@"C:\data.grf");

foreach (var entry in grf.FileTable.EntriesInDirectory(@"C:\texture", SearchOption.TopDirectoryOnly)) {
	if (entry.RelativePath.IsExtension(".bmp")) {
		// Using GrfImage requires additional configuration, more on that later.
		GrfImage image = new GrfImage(entry);

		image.Convert(GrfImageType.Bgra32);
		image.Save(GrfPath.Combine(@"C:\test\", entry.RelativePath));
	}
}
```


```
Heya,

I've been receiving a lot of messages regarding encryption (again), so I wanted to clarify a few things:

- Your PMs regarding encryption matters will be redirected to this post.
- I will not be making a paid version of GRF Editor for encryption.
- Yes, I am aware there is someone selling a decryption tool.
- No, this has nothing to do with GRF Editor being open source. GRF Editor has been open source for a very long time.
- No, I will not be updating the encryption DLL for the foreseeable future.
- GRF Editor is open source (https://github.com/Tokeiburu/GRFEditor), there's no need to pay to see the source (a few people made this request already...).
- I am considering removing the default encryption feature until there is an alternative.
As of GRF Editor 1.8.7.2 and above, it is now possible to make your own custom encryption library and link it with GRF Editor (this is meant for developers). Here is the project file:

comp_x86.rar
88.93 kB
 · 
8 downloads

This is a C++ project made for Visual Studio 2022. You can use a lower toolset for older clients as it was originally made using Visual Studio 2010, which should be more than enough. The respective Visual C++ Redistributables should be installed (x86 for the client and x64 for GRF Editor).

When making the encrypt.dll for GRF Editor:

- Use "Release" and "x64".
  - The DLL will be compiled at comp_x86\Release\x64_GrfEditor\encrypt.dll
- "#define GRF_EDITOR" in cps.h must be defined in cps.h. This exposes the encrypt/decrypt functions for GRF Editor.
- The DLL can then be loaded in GRF Editor from Tools > Settings > Application > Encryption method...
- GRF Editor uses these methods for encryption/decryption:
  - DLL int encrypt(BYTE* key, UInt32 key_len, BYTE* compressed_data, UInt32 compressed_len, UInt32 uncompressed_len);
  - DLL int decrypt(BYTE* key, UInt32 key_len, BYTE* compressed_data, UInt32 compressed_len, UInt32 uncompressed_len);
  - The compressed_data length cannot be modified. The uncompressed_len can be used for your encryption algorithm, but otherwise it isn't useful for anything.
  - The encryption happens after the compression (raw data > zlib/lzma compression > encryption). It is the final method applied on the data. You can make a custom compression library to change the compression data length.
  - The "key" parameter is the one used by GRF Editor when loading an encryption key from the software. You're free to ignore this parameter if you're not going to use this. The key from GRF Editor is 256 bytes in length.

When making the cps.dll for your client:

- Use "Release" and "Win32".
  - The DLL will be compiled at comp_x86\Release\x86_Client\cps.dll
- "#define GRF_EDITOR" in cps.h must NOT be defined. Comment it out if it isn't automatically undefined (using Win32 should undefine it by default).
- The most important function is "void decryptSub(BYTE* key, UInt32 key_len, BYTE* data, UInt32 data_len, UInt32 seed)" in cps.cpp. The function decrypts the data in the GRF using the key provided by GetGrfEncryptionKey(). This part needs to be defined on your end as the key provided is just a dummy one. Hide it, do whatever you want. By default, the encryption uses an RSA encryption method.
- This project has no protection on the DLL whatsoever. That part is entirely up to you.
  - Make sure the client-side version of your cps.dll cannot be loaded in GRF Editor directly as a custom compression library as this would... make the whole thing rather pointless.
- The methods of interest for the client side would be:
  - DLL int uncompress(BYTE* output_data, UInt32* output_len, const BYTE* compressed_data, UInt32 compressed_len)
  - int _encryptedUncompress(BYTE* uncomp, UInt32* uncompLength, const BYTE* comp, UInt32 compLength)

If you're a developer and you think your version is better than the one provided by GRF Editor, feel free to PM me as I'll make it as a default option (I will request to see the source however). Likewise, you're also free to sell those encryption DLLs if you want to, that's entirely up to you (I will not be responsible for issues related to third party encryption libraries).
```