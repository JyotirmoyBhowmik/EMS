using OtpNet;
using QRCoder;

namespace AuthService.Services;

public interface IMfaService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string userEmail, string secret);
    byte[] GenerateQrCodeImage(string uri);
    bool ValidateCode(string secret, string code);
}

public class TotpMfaService : IMfaService
{
    private const string Issuer = "SCADA System";

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUri(string userEmail, string secret)
    {
        return $"otpauth://totp/{Issuer}:{userEmail}?secret={secret}&issuer={Issuer}";
    }

    public byte[] GenerateQrCodeImage(string uri)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public bool ValidateCode(string secret, string code)
    {
        var secretBytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(secretBytes);
        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
    }
}
