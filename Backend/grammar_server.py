from google.protobuf.empty_pb2 import Empty
import sys

import grpc
from concurrent import futures

import grammar_pb2
import grammar_pb2_grpc

from AI.Pipeline.pipeline import grammar_pipeline


class GrammarServicer(grammar_pb2_grpc.GrammarServiceServicer):

    print("grammar_pb2 loaded from:", grammar_pb2.__file__)
    print("sys.path =", sys.path)
    print("Has ClearTextError:", hasattr(grammar_pb2, "ClearTextError"))

    def CheckGrammar(self, request, context):
        text = request.text
        result = grammar_pipeline(text)

        errors = [
            grammar_pb2.ClearTextError(
                type=e["type"],
                token=e["token"],
                index=e["index"],
                suggestions=e.get("suggestions", []),
            )
            for e in result.get("errors", [])
        ]

        return grammar_pb2.GrammarResponse(
            corrected_text=result.get("text", ""),
            tokens=result.get("tokens", []),
            errors=errors,
        )

    def Ping(self, request, context):
        return Empty()


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    grammar_pb2_grpc.add_GrammarServiceServicer_to_server(GrammarServicer(), server)

    # IMPORTANT to bind to both IPv4 and IPv6
    server.add_insecure_port("127.0.0.1:50051")
    print("gRPC Grammar Server running on port 50051")

    server.start()
    server.wait_for_termination()


if __name__ == "__main__":
    serve()
