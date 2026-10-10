using Application.Localization;

namespace MonthSpendings.Pages
{
    public record PrivacyPolicySection(string Title, string Body);

    /// <summary>Per-language text of the public privacy policy page. "{email}" in a body is rendered as the support mail link.</summary>
    public record PrivacyPolicyContent(
        string Label,
        string Title,
        string LastUpdated,
        string Intro,
        string? TranslationNotice,
        string FooterRights,
        string FooterContact,
        PrivacyPolicySection[] Sections)
    {
        public static PrivacyPolicyContent For(string language) => language switch
        {
            SupportedLanguages.Bulgarian => Bulgarian,
            SupportedLanguages.Spanish => Spanish,
            _ => English
        };

        private static readonly PrivacyPolicyContent English = new(
            "Legal",
            "Privacy Policy",
            "Last updated: May 2025",
            "Your privacy matters to us. This policy explains what data Tavira collects, how it is used, and the choices you have regarding your information.",
            null,
            "All rights reserved.",
            "Contact",
            [
                new("Information We Collect", "We collect information you provide directly, including your name, email address, and profile photo obtained via Google Sign-In. Usage data such as feature interactions and error logs may be collected to improve the service."),
                new("How We Use Your Information", "Your information is used to provide, maintain, and improve the Tavira budgeting service. We use your data to personalize your experience, send you relevant notifications, and respond to support requests."),
                new("Data Sharing", "We do not sell, rent, or trade your personal information to third parties. We may share data with trusted service providers who assist in operating Tavira, subject to strict confidentiality agreements. We may disclose information if required to do so by law or to protect the rights of our users."),
                new("Data Security", "We implement industry-standard security measures including encryption in transit (TLS) and at rest to protect your data. Access to user data is restricted to authorized personnel only. While we strive to protect your information, no method of transmission over the internet is completely secure."),
                new("Data Retention", "We retain your personal data for as long as your account remains active or as needed to provide the service. If you delete your account, your data will be permanently removed within 30 days, except where retention is required by law."),
                new("Your Rights", "You have the right to access, correct, or delete the personal information we hold about you. To exercise any of these rights, contact us at {email}."),
                new("Children's Privacy", "Tavira is not directed to individuals under the age of 16. We do not knowingly collect personal information from children. If we become aware that a child has provided us with personal information, we will delete it promptly."),
                new("Changes to This Policy", "We may update this Privacy Policy periodically to reflect changes in our practices or applicable regulations. Material changes will be communicated through the app or via email. Your continued use of Tavira after changes are posted constitutes your acceptance of the updated policy."),
                new("Contact Us", "If you have any questions about this Privacy Policy or how we handle your data, please contact us at {email}. We are committed to addressing privacy concerns promptly and transparently."),
            ]);

        private static readonly PrivacyPolicyContent Bulgarian = new(
            "Правна информация",
            "Политика за поверителност",
            "Последна актуализация: май 2025 г.",
            "Вашата поверителност е важна за нас. Тази политика обяснява какви данни събира Tavira, как се използват и какви възможности имате по отношение на вашата информация.",
            "Това е превод. При разминаване с оригиналния текст на английски език предимство има английската версия.",
            "Всички права запазени.",
            "Контакт",
            [
                new("Информация, която събираме", "Събираме информацията, която ни предоставяте директно, включително вашето име, имейл адрес и профилна снимка, получени чрез вход с Google. Може да събираме и данни за използването, като взаимодействия с функции и регистри на грешки, за да подобряваме услугата."),
                new("Как използваме вашата информация", "Вашата информация се използва за предоставяне, поддръжка и подобряване на услугата за бюджетиране Tavira. Използваме данните ви, за да персонализираме преживяването ви, да ви изпращаме подходящи известия и да отговаряме на запитвания за поддръжка."),
                new("Споделяне на данни", "Не продаваме, не отдаваме под наем и не търгуваме с вашата лична информация с трети страни. Може да споделяме данни с доверени доставчици на услуги, които помагат за работата на Tavira, при стриктни споразумения за поверителност. Може да разкрием информация, ако това се изисква по закон или за защита на правата на нашите потребители."),
                new("Сигурност на данните", "Прилагаме стандартни за индустрията мерки за сигурност, включително криптиране при пренос (TLS) и при съхранение, за да защитим данните ви. Достъпът до потребителски данни е ограничен само до упълномощен персонал. Въпреки че се стремим да защитим информацията ви, нито един метод за пренос през интернет не е напълно сигурен."),
                new("Съхранение на данни", "Съхраняваме личните ви данни, докато акаунтът ви е активен или докато е необходимо за предоставяне на услугата. Ако изтриете акаунта си, данните ви ще бъдат окончателно премахнати в рамките на 30 дни, освен когато съхранението им се изисква по закон."),
                new("Вашите права", "Имате право на достъп, коригиране или изтриване на личната информация, която съхраняваме за вас. За да упражните някое от тези права, свържете се с нас на {email}."),
                new("Поверителност на децата", "Tavira не е предназначено за лица под 16 години. Не събираме съзнателно лична информация от деца. Ако разберем, че дете ни е предоставило лична информация, ще я изтрием незабавно."),
                new("Промени в тази политика", "Може периодично да актуализираме тази Политика за поверителност, за да отразим промени в практиките ни или в приложимите разпоредби. За съществени промени ще ви уведомим чрез приложението или по имейл. Продължаването на използването на Tavira след публикуване на промените означава, че приемате актуализираната политика."),
                new("Свържете се с нас", "Ако имате въпроси относно тази Политика за поверителност или как обработваме данните ви, свържете се с нас на {email}. Ангажирани сме да отговаряме на въпроси за поверителността бързо и прозрачно."),
            ]);

        private static readonly PrivacyPolicyContent Spanish = new(
            "Legal",
            "Política de privacidad",
            "Última actualización: mayo de 2025",
            "Tu privacidad nos importa. Esta política explica qué datos recopila Tavira, cómo se utilizan y qué opciones tienes respecto a tu información.",
            "Esta es una traducción. En caso de discrepancia con el texto original en inglés, prevalecerá la versión en inglés.",
            "Todos los derechos reservados.",
            "Contacto",
            [
                new("Información que recopilamos", "Recopilamos la información que nos proporcionas directamente, incluidos tu nombre, dirección de correo electrónico y foto de perfil obtenidos mediante el inicio de sesión con Google. También podemos recopilar datos de uso, como interacciones con funciones y registros de errores, para mejorar el servicio."),
                new("Cómo usamos tu información", "Tu información se utiliza para proporcionar, mantener y mejorar el servicio de presupuestos de Tavira. Usamos tus datos para personalizar tu experiencia, enviarte notificaciones relevantes y responder a solicitudes de soporte."),
                new("Compartición de datos", "No vendemos, alquilamos ni intercambiamos tu información personal con terceros. Podemos compartir datos con proveedores de servicios de confianza que ayudan a operar Tavira, sujetos a estrictos acuerdos de confidencialidad. Podemos divulgar información si la ley lo exige o para proteger los derechos de nuestros usuarios."),
                new("Seguridad de los datos", "Aplicamos medidas de seguridad estándar del sector, incluido el cifrado en tránsito (TLS) y en reposo, para proteger tus datos. El acceso a los datos de los usuarios está restringido únicamente al personal autorizado. Aunque nos esforzamos por proteger tu información, ningún método de transmisión por internet es completamente seguro."),
                new("Conservación de datos", "Conservamos tus datos personales mientras tu cuenta permanezca activa o mientras sea necesario para prestar el servicio. Si eliminas tu cuenta, tus datos se eliminarán de forma permanente en un plazo de 30 días, salvo cuando la ley exija conservarlos."),
                new("Tus derechos", "Tienes derecho a acceder, corregir o eliminar la información personal que tenemos sobre ti. Para ejercer cualquiera de estos derechos, contáctanos en {email}."),
                new("Privacidad de los menores", "Tavira no está dirigida a personas menores de 16 años. No recopilamos conscientemente información personal de menores. Si descubrimos que un menor nos ha proporcionado información personal, la eliminaremos de inmediato."),
                new("Cambios en esta política", "Podemos actualizar esta Política de privacidad periódicamente para reflejar cambios en nuestras prácticas o en la normativa aplicable. Los cambios importantes se comunicarán a través de la aplicación o por correo electrónico. Si sigues usando Tavira después de que se publiquen los cambios, aceptas la política actualizada."),
                new("Contáctanos", "Si tienes alguna pregunta sobre esta Política de privacidad o sobre cómo tratamos tus datos, contáctanos en {email}. Nos comprometemos a atender las cuestiones de privacidad de forma rápida y transparente."),
            ]);
    }
}
