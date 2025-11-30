"""
🛒 E-COMMERCE MCP - TES OUTILS RÉELS
"""
from fastmcp import FastMCP
import logging

# Logger
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Instance MCP (PAS de session_timeout)
mcp = FastMCP(name="ecommerce-devtools")

# ✅ TES OUTILS IMPORTÉS
from tools.db_client import DatabaseClientTool
from tools.performance_analyzer import PerformanceAnalyzerTool
from tools.interactive_debugger import InteractiveDebuggerTool

db_tool = DatabaseClientTool()
perf_tool = PerformanceAnalyzerTool()
debug_tool = InteractiveDebuggerTool()

@mcp.tool()
async def execute_db_query(query: str, limit: int = 100) -> str:
    """🗄️ PostgreSQL RÉEL - asyncpg"""
    return await db_tool.execute_query(query, limit)

@mcp.tool()
async def analyze_endpoint(endpoint: str, method: str = "GET", payload: str = "{}") -> str:
    """🔍 API RÉEL - aiohttp"""
    return await perf_tool.analyze(endpoint, method, payload)

@mcp.tool()
async def debug_eval(code: str) -> str:
    """🐛 C# Debug RÉEL"""
    return await debug_tool.evaluate(code)

@mcp.tool()
async def list_endpoints() -> dict:
    """📋 Endpoints API"""
    return {
        "endpoints": ["/api/products", "/api/orders", "/api/categories"],
        "total": 25
    }

# === DÉMARRAGE ===
if __name__ == "__main__":
    logger.info("🚀 🛒 MCP - TES OUTILS ✅")
    mcp.run(
        transport="http",
        host="0.0.0.0",
        port=8080,
        path="/mcp"
    )