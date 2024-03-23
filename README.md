# InstrukcijeDotNet backend

## Implementirano:
- dodane sve putanje za dohvaćanj, i stavljanje entitete te pu i delete putanje samo za stvari koje su mi trebale za implementaciju
- povezano sa bazom podataka, uspješno spremanje i dohvaćanje podataka - korišteni entiteti
- baza se sastoji od tablica: students, professors, instructionDates, subjects i professorsubjects
- povezan frontend end s backendom
- funkcionalnosti:
   - omogućen login i registracija studenata i profesora
   - omogućen prikaz instuktora i dogovaranja instrukcija s istim
   - omogućeno uređivanje podataka trernutno ulogiranog korsnik
   - omogućeno da instruktori ne mogu od instruktora tražiti instukcije
   - dodan kalendar za svakog korisnika koji prikazuje njegov raspored instrukcija. Organiziran je po bojama: plava(instrukcije koje čekaju povbratnu informaciju), crvena(instrukcije koje su prošle) i zelena(nadolazeće instrukcije)
   - dodan graf koji prikazuje profesore sa najviše održanih/zakazanih instrukcija
   - dodane postavke u kojima se može uspješno uređivati podaci korisnika i dodati slika profila (koristio sam linkove slika s google)
   - omogućeno da samo profesor može kreirati nove predmete i da se oni također naknadno mogu pridružiti nekom predmetu
## Neimplementirano:
   - nedostaje mogućnost da profesor odobrni zahtjev za termin instrukcije
   - ove napomene na kraju readme vašeg projekta sam prekasno vidio, tako da se nažalost mogu kreirati korisnici s istim mail adresama, te nema restrikcija na broj instrukcija za dogovoriti

## Upute za pokretanje
U frontend pokrenut i vsc-u, npm run dev komanda (localhost:5173 je stranica), prije toga backend pokrenuti u vs sa komandom dotnet run (localhost:5000/api je url backemda) (mora se nalaziti u unutrašnjem dotNetInstrukcije folderu)


Sve u svemu jedno izvrsno iskustvo, volio bih da je bilo još malo više vremena (jer zbog obaveza nisam se mogao maksimlano fokusirati) i siguran sam da bi rezultati stranice bili još i bolji
Velikeee pohvale organizatorima na odličnim predavanjima i trudu. Baš se vidi da ste dali sve od sebe. Radoionica je bilo vrlo korisna i bilo mi je užitak biti jedan od polaznika! :)

Link na primjer korištenja stranice za studenta i profesora: https://drive.google.com/drive/folders/1i39UrwQHpefUKliwjPqCpVVha_AZtxZK
