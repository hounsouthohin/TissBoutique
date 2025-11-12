# 🛍️ ECommerce Application

Application e-commerce complète construite avec **ASP.NET Core 8** (Backend) et **React + TypeScript** (Frontend).

---

## 🚀 Technologies

### Backend
- ASP.NET Core 8 Web API
- Entity Framework Core 8
- PostgreSQL 15
- Redis (Cache & Sessions)
- ASP.NET Identity (Authentication)
- JWT Bearer Tokens
- SignalR (WebSockets temps réel)
- Stripe (Paiements)
- AutoMapper
- FluentValidation
- Serilog

### Frontend
- React 18
- TypeScript 5
- Vite 5
- TailwindCSS 3
- Redux Toolkit
- React Router 6
- Axios
- SignalR Client
- React Hook Form
- Zod (Validation)

### DevOps
- Docker & Docker Compose
- GitHub Actions (CI/CD)
- PostgreSQL (Base de données)
- Redis (Cache)

---

## 📋 Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

---

## ⚡ Installation Rapide

### 1️⃣ Cloner le projet

\\\ash
git clone <votre-repo>
cd ECommerceApp
\\\

### 2️⃣ Configuration des variables d'environnement

Créer un fichier \.env\ à la racine :

\\\env
# Stripe
STRIPE_SECRET_KEY=sk_test_your_key_here
STRIPE_PUBLISHABLE_KEY=pk_test_your_key_here

# Email (Gmail)
EMAIL_USER=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
\\\

### 3️⃣ Démarrer avec Docker

\\\ash
# Démarrer tous les services
docker-compose up -d

# Voir les logs
docker-compose logs -f

# Arrêter les services
docker-compose down
\\\

**URLs après démarrage :**
- Frontend : http://localhost:3000
- Backend API : http://localhost:5000
- Swagger : http://localhost:5000/swagger
- PgAdmin : http://localhost:5050

---

## 🛠️ Développement Local (Sans Docker)

### Backend

\\\ash
cd backend

# Restaurer les packages
dotnet restore

# Appliquer les migrations
dotnet ef database update --project ECommerce.Infrastructure --startup-project ECommerce.API

# Démarrer l'API
cd ECommerce.API
dotnet run
\\\

### Frontend

\\\ash
cd frontend

# Installer les dépendances
npm install

# Démarrer en mode dev
npm run dev
\\\

---

## 🗄️ Base de données

### Créer une nouvelle migration

\\\ash
cd backend
dotnet ef migrations add MigrationName --project ECommerce.Infrastructure --startup-project ECommerce.API
\\\

### Appliquer les migrations

\\\ash
dotnet ef database update --project ECommerce.Infrastructure --startup-project ECommerce.API
\\\

### Supprimer la dernière migration

\\\ash
dotnet ef migrations remove --project ECommerce.Infrastructure --startup-project ECommerce.API
\\\

---

## 🧪 Tests

### Exécuter tous les tests

\\\ash
cd backend
dotnet test
\\\

### Tests avec couverture de code

\\\ash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
\\\

---

## 📁 Structure du Projet

\\\
ECommerceApp/
├── backend/
│   ├── ECommerce.API/              # Couche présentation (Controllers, Middleware)
│   ├── ECommerce.Application/      # Logique métier (Services, DTOs)
│   ├── ECommerce.Domain/           # Entités & interfaces
│   ├── ECommerce.Infrastructure/   # Accès données (DbContext, Repositories)
│   └── ECommerce.Tests/            # Tests unitaires & intégration
├── frontend/
│   ├── src/
│   │   ├── components/             # Composants réutilisables
│   │   ├── pages/                  # Pages principales
│   │   ├── services/               # API calls
│   │   ├── store/                  # Redux state
│   │   └── types/                  # TypeScript types
│   └── public/
├── docker-compose.yml
├── .gitignore
└── README.md
\\\

---

## 🔐 Utilisateur par défaut

Après l'initialisation de la base de données :

**Email :** admin@ecommerce.com  
**Mot de passe :** Admin123!  
**Rôle :** Admin

---

## 🌐 API Endpoints

### Authentication
- POST \/api/auth/register\ - Inscription
- POST \/api/auth/login\ - Connexion
- POST \/api/auth/refresh-token\ - Rafraîchir le token
- GET \/api/auth/me\ - Profil utilisateur

### Products
- GET \/api/products\ - Liste des produits
- GET \/api/products/{id}\ - Détail produit
- GET \/api/products/slug/{slug}\ - Produit par slug
- POST \/api/products\ - Créer produit (Admin)
- PUT \/api/products/{id}\ - Modifier produit (Admin)
- DELETE \/api/products/{id}\ - Supprimer produit (Admin)

### Cart
- GET \/api/cart\ - Panier utilisateur
- POST \/api/cart/items\ - Ajouter au panier
- PUT \/api/cart/items/{productId}\ - Modifier quantité
- DELETE \/api/cart/items/{productId}\ - Retirer du panier

### Orders
- GET \/api/orders\ - Commandes utilisateur
- GET \/api/orders/{id}\ - Détail commande
- POST \/api/orders\ - Créer commande
- PUT \/api/orders/{id}/cancel\ - Annuler commande

### Payments
- POST \/api/payments/create-intent\ - Créer intention de paiement
- POST \/api/payments/confirm\ - Confirmer paiement

---

## 📦 Build Production

### Backend

\\\ash
cd backend/ECommerce.API
dotnet publish -c Release -o ./publish
\\\

### Frontend

\\\ash
cd frontend
npm run build
\\\

---

## 🤝 Contribution

1. Fork le projet
2. Créer une branche (\git checkout -b feature/AmazingFeature\)
3. Commit les changements (\git commit -m 'Add AmazingFeature'\)
4. Push vers la branche (\git push origin feature/AmazingFeature\)
5. Ouvrir une Pull Request

---

## 📝 License

Ce projet est sous licence MIT.

---

## 👤 Auteur

**Votre Nom**

- GitHub: [@votre-username](https://github.com/votre-username)
- Email: votre-email@example.com

---

## 🙏 Remerciements

- Anthropic Claude pour l'assistance au développement
- Communauté .NET et React
- Tous les contributeurs open-source

---

**🚀 Bon développement !**
