using Vulicy.Domain;
using Vulicy.Services;

namespace Vulicy.Tests;

public class NameHelpersUnitTests
{
    [Theory]
    [InlineData("вуліца Камароўскае Кальцо", FeatureType.Street, "Камароўскае Кальцо")]
    [InlineData("улица Комаровское Кольцо", FeatureType.Street, "Комаровское Кольцо")]
    [InlineData("2-я вуліца Пржавальскага", FeatureType.Street, "2-я Пржавальскага")]
    [InlineData("праспект Незалежнасці", FeatureType.Avenue, "Незалежнасці")]
    [InlineData("праспэкт Незалежнасьці", FeatureType.Avenue, "Незалежнасьці")]
    [InlineData("проспект Независимости", FeatureType.Avenue, "Независимости")]
    [InlineData("плошча Францішка Багушэвіча", FeatureType.Square, "Францішка Багушэвіча")]
    [InlineData("площадь Франтишка Богушевича", FeatureType.Square, "Франтишка Богушевича")]
    [InlineData("бульвар Мулявіна", FeatureType.Boulevard, "Мулявіна")]
    [InlineData("Набярэжная вуліца", FeatureType.Street, "Набярэжная")]
    [InlineData("набярэжная Францыска Скарыны", FeatureType.Riverside, "Францыска Скарыны")]
    [InlineData("Ракаўская шаша", FeatureType.Highway, "Ракаўская")]
    [InlineData("Раковское шоссе", FeatureType.Highway, "Раковское")]
    [InlineData("Рубаўскае кальцо", FeatureType.Roundabout, "Рубаўскае")]
    [InlineData("Музычны завулак", FeatureType.Alley, "Музычны")]
    [InlineData("Музыкальный переулок", FeatureType.Alley, "Музыкальный")]
    [InlineData("завулак Кузьмы Чорнага", FeatureType.Alley, "Кузьмы Чорнага")]
    [InlineData("переулок Кузьмы Чорного", FeatureType.Alley, "Кузьмы Чорного")]
    [InlineData("2-і завулак Баграціёна", FeatureType.Alley, "2-і Баграціёна")]
    [InlineData("4-ы Арлоўскі завулак", FeatureType.Alley, "4-ы Арлоўскі")]
    [InlineData("Каралінскі праезд", FeatureType.Driveway, "Каралінскі")]
    [InlineData("Каролинский проезд", FeatureType.Driveway, "Каролинский")]
    [InlineData("Брылеўскі тупік", FeatureType.DeadEnd, "Брылеўскі")]
    [InlineData("Брилевский тупик", FeatureType.DeadEnd, "Брилевский")]
    [InlineData("2-і Заходні тупік", FeatureType.DeadEnd, "2-і Заходні")]
    [InlineData("2-і Веласіпедны завулак", FeatureType.Alley, "2-і Веласіпедны")]
    [InlineData("Дняпроўскі спуск", FeatureType.Descent, "Дняпроўскі")]
    [InlineData("Аляксандраўскі сквер", FeatureType.PublicGarden, "Аляксандраўскі")]
    public void ParseOsmCyrillicName_ShouldReturnExpectedTypeAndName(string fullName, FeatureType expectedType, string expectedName)
    {
        var (actualType, actualName) = NameHelpers.ParseOsmCyrillicName(fullName);

        Assert.Equal(expectedType, actualType);
        Assert.Equal(expectedName, actualName);
    }

    [Theory]
    [InlineData("Зенкевич Антон Павлович - учитель, инициатора открытия Остромечевской библиотеки в 1905 году",
                "Зенкевич Антон Павлович — учитель, инициатора открытия Остромечевской библиотеки в 1905 году", true)]
    [InlineData("Иван Васильевич Болдин — советский военачальник, командарм Великой Отечественной войны, генерал-полковник",
                "Иван Васильевич Болдин — советский военачальник, командарм, участник освобождения  г. Гродно от немецко-фашистских захватчиков в годы ВОВ, генерал-полковник", false)] // humanly true, but not by algorithm
    [InlineData("Ян Фабрыцыюс (Jānis Fabriciuss) - расейскі бальшавік і военачальнік. Дачыненьня да Беларусі ня меў",
                "Ян Фабрыцыюс (Jānis Fabriciuss) - расейскі бальшавік і военачальнік. Дачыненьня да Беларусі ня меў.", true)]
    [InlineData("Юрый Сьмірноў (Смирнов) - савецкі вайсковец, ураджэнец Расеі, загінуў на тэрыторыі Беларусі падчас Другой сусьветнай вайны",
                "Юрый Сьмірноў (Юрий Смирнов) - савецкі вайсковец у Другой сусьветнай вайны, удзельнік савецка-нацыстоўскіх баёў за Беларусь у 1944 г.", false)] // humanly true, but not by algorithm
    [InlineData("Эрнст Тэльман (Ernst Thälmann) - нямецкі камуніст",
                "Юзаф Зарэцкі - расейскі вайсковы інжынэр XIX стагодзьдзя, ураджэнец Горадні", false)]
    [InlineData("Уладзімір Ярмак - савецкі жаўнер беларускага паходжаньня, які загінуў падчас Другой сусьветнай вайны",
                "Уладзімір Ярмак - савецкі жаўнер, які загінуў падчас Другой сусьветнай вайны, нарадзіўся ў Менску", false)] // humanly true, but not by algorithm
    [InlineData("Владимир Михайлович Комаров — лётчик-космонавт № 7, инженер-полковник, дважды Герой Советского Союза. Командир первого в мире экипажа космического корабля",
                "Владимир Михайлович Комаров — лётчик-космонавт № 7, дважды Герой Советского Союз, инженер-полковник. Командир первого в мире экипажа космического корабля", true)]
    [InlineData("test", "eee", false)]
    public void IsSimilar_ShouldReturnExpectedResult(string s1, string s2, bool expectedResult)
    {
        var actualResult = NameHelpers.IsSimilar(s1, s2, 0.7);

        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("названа ў гонар Гагарына", "названа ў гонар Юрыя Гагарына", 0.8, true)]
    [InlineData("названа ў гонар Гагарына", "названа ў гонар Юрыя Гагарына", 0.9, false)]
    public void IsSimilar_WithThreshold_ShouldReturnExpectedResult(string s1, string s2, double threshold, bool expectedResult)
    {
        var actualResult = NameHelpers.IsSimilar(s1, s2, threshold);

        Assert.Equal(expectedResult, actualResult);
    }

    [Theory]
    [InlineData("Цнянская", null)]
    [InlineData("2-і Веласіпедны", "Веласіпедны 2-і")]
    [InlineData("2-й Велосипедный", "Велосипедный 2-й")]
    [InlineData("2-я Шостая Лінія", "Шостая Лінія 2-я")]
    [InlineData("2-я Шестая Линия", "Шестая Линия 2-я")]
    [InlineData("1-я Радыятарная", "Радыятарная 1-я")]
    [InlineData("4-ы Арлоўскі", "Арлоўскі 4-ы")]
    public void NamesMatch_Test(string name, string? expectedResult)
    {
        var actualResult = NameHelpers.TryGetAlternativeName(name);

        Assert.Equal(expectedResult, actualResult);
    }
}