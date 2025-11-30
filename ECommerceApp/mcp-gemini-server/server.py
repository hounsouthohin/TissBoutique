"""
🛒 E-COMMERCE MCP - TES OUTILS RÉELS
"""
from fastmcp import FastMCP
import logging

# Logger
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Instance MCP
mcp = FastMCP(name="ecommerce-devtools")

# ✅ IMPORT TES CLASSES RÉELLES
from tools.db_client import DatabaseClientTool
from tools.performance_analyzer import PerformanceAnalyzerTool
from tools.interactive_debugger import InteractiveDebuggerTool

# ✅ INITIALISE TES OUTILS
db_tool = DatabaseClientTool()
perf_tool = PerformanceAnalyzerTool()
debug_tool = InteractiveDebuggerTool()

# === TES OUTILS RÉELS EN DIRECT ===
@mcp.tool()
async def execute_db_query(query: str, limit: int = 100) -> str:
    """🗄️ **RÉEL** PostgreSQL - asyncpg"""
    return await db_tool.execute_query(query, limit)

@mcp.tool()
async def analyze_endpoint(endpoint: str, method: str = "GET", payload: str = "{}") -> str:
    """🔍 **RÉEL** HTTP aiohttp + Métriques"""
    return await perf_tool.analyze(endpoint, method, payload)

@mcp.tool()
async def debug_eval(code: str) -> str:
    """🐛 **RÉEL** Debug C#"""
    return await debug_tool.evaluate(code)

@mcp.tool()
async def list_endpoints() -> dict:
    """📋 Liste endpoints API"""
    return {
        "endpoints": ["/api/products", "/api/orders", "/api/categories"],
        "total": 25
    }

# === DÉMARRAGE 7 ÉTAPES ===
if __name__ == "__main__":
    logger.info("🚀 🛒 MCP - TES OUTILS RÉELS ✅")
    mcp.run(
        transport="http",
        host="0.0.0.0",
        port=8080,
        path="/mcp"
    )