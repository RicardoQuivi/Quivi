using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Quivi.OAuth2.Controllers
{
    [Route("connect/.well-known/jwks.json")]
    public class JwksController : ControllerBase
    {
        public JwksController()
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetJwks()
        {
            // Get the public key from the RSA key
            RSA rsa = RSA.Create(2048);
            var publicKey = rsa.ExportParameters(false); // false to get only the public part

            var jwk = new JsonWebKey
            {
                Kty = "RSA",
                E = Base64UrlEncoder.Encode(publicKey.Exponent),
                N = Base64UrlEncoder.Encode(publicKey.Modulus)
            };

            // Create the JWKS (JSON Web Key Set) containing the public key
            var jwks = new JsonWebKeySet();
            jwks.Keys.Add(jwk); // Add the JWK to the Keys collection
            return Ok(jwks);
        }
    }
}
