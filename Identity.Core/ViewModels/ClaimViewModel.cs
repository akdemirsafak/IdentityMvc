namespace Identity.Core.ViewModels;

public class ClaimViewModel
{
    //Claims .net e özel değil globaldir.oAuth 2.0 ile 3th party authentication kullandığımızda bize claims gönderilir.Bu sebeple claim'i dağıtanı da property olarak ekleyeceğiz.
    public required string Issuer { get; set; } //Claim dağıtıcı. Kendi sistemimizden geliyorsa Local Authority görülür.Kullanıcının hangi yöntemle login olduğunu anlamamız açısından önemlidir.
    public required string Type { get; set; }
    public required string Value { get; set; }

}